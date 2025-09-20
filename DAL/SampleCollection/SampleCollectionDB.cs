using eMedLis.Models.SampleCollection;
using eMedLis.Models.PatientBilling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.DAL.SampleCollection
{
    public class SampleCollectionDB
    {
        private readonly string _connectionString;

        public SampleCollectionDB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;
        }

        public SampleCollectionViewModel GetBillForCollection(int billSummaryId)
        {
            var viewModel = new SampleCollectionViewModel();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Get Bill and Patient Info
                var cmd = new SqlCommand(@"
                    SELECT bs.*, pi.*, bd.InvId, bd.InvName, bd.Rate, LTS.ItemName as SpecimenName, LTV.ItemName as VacutainerName
                    FROM BillSummary bs
                    INNER JOIN PatientInfo pi ON bs.PatientInfoId = pi.PatientInfoId
                    LEFT JOIN BillDetail bd ON bs.BillSummaryId = bd.BillSummaryId
                    LEFT JOIN InvMaster im ON bd.InvId = im.Id
                    INNER JOIN LookupTable LTS ON IM.SpecimenId = LTS.Id
                    INNER JOIN LookupTable LTV ON IM.VacutainerId = LTV.Id
                    WHERE bs.BillSummaryId = @BillSummaryId", connection);

                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                using (var reader = cmd.ExecuteReader())
                {
                    var billDetails = new List<BillDetail>();

                    while (reader.Read())
                    {
                        if (viewModel.PatientInfo == null)
                        {
                            viewModel.PatientInfo = new PatientInfo
                            {
                                PatientInfoId = Convert.ToInt32(reader["PatientInfoId"]),
                                UHID = reader["UHID"]?.ToString(),
                                PatName = reader["PatName"].ToString(),
                                MobileNo = reader["MobileNo"].ToString(),
                                Age = Convert.ToInt32(reader["Age"]),
                                Gender = reader["Gender"]?.ToString(),
                                Area = reader["Area"]?.ToString(),
                                City = reader["City"]?.ToString()
                            };

                            viewModel.BillSummary = new BillSummary
                            {
                                BillSummaryId = Convert.ToInt32(reader["BillSummaryId"]),
                                BillNo = reader["BillNo"]?.ToString(),
                                BillDate = Convert.ToDateTime(reader["BillDate"]),
                                NetAmount = Convert.ToDecimal(reader["NetAmount"])
                            };
                        }

                        if (reader["InvId"] != DBNull.Value)
                        {
                            billDetails.Add(new BillDetail
                            {
                                InvId = reader["InvId"].ToString(),
                                InvName = reader["InvName"]?.ToString(),
                                Rate = Convert.ToDecimal(reader["Rate"]),
                                SpecimenType = reader["SpecimenName"]?.ToString(),
                                ContainerType = reader["VacutainerName"]?.ToString()
                            });
                        }
                    }

                    viewModel.BillDetails = billDetails;
                }
            }

            return viewModel;
        }

        public List<ContainerMaster> GetActiveContainers()
        {
            var containers = new List<ContainerMaster>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT * FROM ContainerMaster WHERE Active = 1 ORDER BY ContainerName", connection);
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        containers.Add(new ContainerMaster
                        {
                            ContainerId = Convert.ToInt32(reader["ContainerId"]),
                            ContainerName = reader["ContainerName"].ToString(),
                            ContainerCode = reader["ContainerCode"].ToString(),
                            CapColor = reader["CapColor"]?.ToString(),
                            Volume = reader["Volume"]?.ToString(),
                            Additive = reader["Additive"]?.ToString(),
                            StorageTemp = reader["StorageTemp"]?.ToString(),
                            ExpiryDays = Convert.ToInt32(reader["ExpiryDays"])
                        });
                    }
                }
            }

            return containers;
        }

        public string SaveSampleCollection(SampleCollectionModel sampleCollection, List<SampleCollectionDetail> sampleDetails)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Generate collection barcode
                        string collectionBarcode;
                        var barcodeCmd = new SqlCommand("usp_GenerateCollectionBarcode", connection, transaction);
                        barcodeCmd.CommandType = CommandType.StoredProcedure;
                        barcodeCmd.Parameters.AddWithValue("@CollectionDate", sampleCollection.CollectionDate.Date);
                        var barcodeParam = new SqlParameter("@SampleBarcode", SqlDbType.VarChar, 30)
                        {
                            Direction = ParameterDirection.Output
                        };
                        barcodeCmd.Parameters.Add(barcodeParam);
                        barcodeCmd.ExecuteNonQuery();
                        collectionBarcode = barcodeParam.Value.ToString();

                        // Insert sample collection master
                        var masterCmd = new SqlCommand(@"
                            INSERT INTO SampleCollection 
                            (BillSummaryId, PatientInfoId, CollectionDate, CollectionTime, CollectedBy, 
                             CollectionBarcode, CollectionStatus, Priority, Remarks, HomeCollection, 
                             PatientAddress, CollectionCharges, CreatedBy)
                            VALUES 
                            (@BillSummaryId, @PatientInfoId, @CollectionDate, @CollectionTime, @CollectedBy,
                             @CollectionBarcode, @CollectionStatus, @Priority, @Remarks, @HomeCollection,
                             @PatientAddress, @CollectionCharges, @CreatedBy);
                            SELECT SCOPE_IDENTITY();", connection, transaction);

                        masterCmd.Parameters.AddWithValue("@BillSummaryId", sampleCollection.BillSummaryId);
                        masterCmd.Parameters.AddWithValue("@PatientInfoId", sampleCollection.PatientInfoId);
                        masterCmd.Parameters.AddWithValue("@CollectionDate", sampleCollection.CollectionDate);
                        masterCmd.Parameters.AddWithValue("@CollectionTime", sampleCollection.CollectionTime);
                        masterCmd.Parameters.AddWithValue("@CollectedBy", sampleCollection.CollectedBy);
                        masterCmd.Parameters.AddWithValue("@CollectionBarcode", collectionBarcode);
                        masterCmd.Parameters.AddWithValue("@CollectionStatus", sampleCollection.CollectionStatus);
                        masterCmd.Parameters.AddWithValue("@Priority", sampleCollection.Priority);
                        masterCmd.Parameters.AddWithValue("@Remarks", sampleCollection.Remarks ?? "");
                        masterCmd.Parameters.AddWithValue("@HomeCollection", sampleCollection.HomeCollection);
                        masterCmd.Parameters.AddWithValue("@PatientAddress", sampleCollection.PatientAddress ?? "");
                        masterCmd.Parameters.AddWithValue("@CollectionCharges", sampleCollection.CollectionCharges);
                        masterCmd.Parameters.AddWithValue("@CreatedBy", sampleCollection.CreatedBy);

                        int sampleCollectionId = Convert.ToInt32(masterCmd.ExecuteScalar());

                        // Insert sample details
                        foreach (var detail in sampleDetails)
                        {
                            // Generate individual sample barcode
                            string sampleBarcode;
                            var detailBarcodeCmd = new SqlCommand("usp_GenerateSampleBarcode", connection, transaction);
                            detailBarcodeCmd.CommandType = CommandType.StoredProcedure;
                            detailBarcodeCmd.Parameters.AddWithValue("@CollectionDate", sampleCollection.CollectionDate.Date);
                            detailBarcodeCmd.Parameters.AddWithValue("@InvCode", "TST"); // You can use investigation code
                            var detailBarcodeParam = new SqlParameter("@SampleBarcode", SqlDbType.VarChar, 30)
                            {
                                Direction = ParameterDirection.Output
                            };
                            detailBarcodeCmd.Parameters.Add(detailBarcodeParam);
                            detailBarcodeCmd.ExecuteNonQuery();
                            sampleBarcode = detailBarcodeParam.Value.ToString();

                            var detailCmd = new SqlCommand(@"
                                INSERT INTO SampleCollectionDetail 
                                (SampleCollectionId, InvMasterId, InvestigationName, SpecimenType, ContainerType,
                                 SampleBarcode, CollectionInstructions, FastingRequired, SpecialInstructions, SampleStatus)
                                VALUES 
                                (@SampleCollectionId, @InvMasterId, @InvestigationName, @SpecimenType, @ContainerType,
                                 @SampleBarcode, @CollectionInstructions, @FastingRequired, @SpecialInstructions, @SampleStatus)",
                                connection, transaction);

                            detailCmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);
                            detailCmd.Parameters.AddWithValue("@InvMasterId", detail.InvMasterId);
                            detailCmd.Parameters.AddWithValue("@InvestigationName", detail.InvestigationName);
                            detailCmd.Parameters.AddWithValue("@SpecimenType", detail.SpecimenType);
                            detailCmd.Parameters.AddWithValue("@ContainerType", detail.ContainerType);
                            detailCmd.Parameters.AddWithValue("@SampleBarcode", sampleBarcode);
                            detailCmd.Parameters.AddWithValue("@CollectionInstructions", detail.CollectionInstructions ?? "");
                            detailCmd.Parameters.AddWithValue("@FastingRequired", detail.FastingRequired);
                            detailCmd.Parameters.AddWithValue("@SpecialInstructions", detail.SpecialInstructions ?? "");
                            detailCmd.Parameters.AddWithValue("@SampleStatus", detail.SampleStatus);

                            detailCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return collectionBarcode;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<SampleCollectionViewModel> GetPendingCollections()
        {
            var collections = new List<SampleCollectionViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(@"
                    SELECT sc.*, pi.PatName, pi.UHID, pi.MobileNo, bs.BillNo,
                           COUNT(scd.SampleDetailId) as TotalSamples,
                           SUM(CASE WHEN scd.SampleStatus = 'Collected' THEN 1 ELSE 0 END) as CollectedSamples
                    FROM SampleCollection sc
                    INNER JOIN PatientInfo pi ON sc.PatientInfoId = pi.PatientInfoId
                    INNER JOIN BillSummary bs ON sc.BillSummaryId = bs.BillSummaryId
                    LEFT JOIN SampleCollectionDetail scd ON sc.SampleCollectionId = scd.SampleCollectionId
                    WHERE sc.CollectionStatus IN ('Pending', 'Partial')
                    GROUP BY sc.SampleCollectionId, sc.BillSummaryId, sc.PatientInfoId, sc.CollectionDate, 
                             sc.CollectionTime, sc.CollectedBy, sc.CollectionBarcode, sc.CollectionStatus,
                             sc.Priority, sc.Remarks, sc.HomeCollection, sc.PatientAddress, sc.CollectionCharges,
                             sc.CreatedDate, sc.CreatedBy, pi.PatName, pi.UHID, pi.MobileNo, bs.BillNo
                    ORDER BY sc.CollectionDate DESC, sc.Priority DESC", connection);

                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        collections.Add(new SampleCollectionViewModel
                        {
                            SampleCollection = new SampleCollectionModel
                            {
                                SampleCollectionId = Convert.ToInt32(reader["SampleCollectionId"]),
                                CollectionBarcode = reader["CollectionBarcode"].ToString(),
                                CollectionDate = Convert.ToDateTime(reader["CollectionDate"]),
                                CollectionTime = (TimeSpan)reader["CollectionTime"],
                                CollectionStatus = reader["CollectionStatus"].ToString(),
                                Priority = reader["Priority"].ToString(),
                                HomeCollection = Convert.ToBoolean(reader["HomeCollection"]),
                                CollectedBy = reader["CollectedBy"].ToString()
                            },
                            PatientInfo = new PatientInfo
                            {
                                PatName = reader["PatName"].ToString(),
                                UHID = reader["UHID"]?.ToString(),
                                MobileNo = reader["MobileNo"].ToString()
                            },
                            BillSummary = new BillSummary
                            {
                                BillNo = reader["BillNo"].ToString()
                            }
                        });
                    }
                }
            }

            return collections;
        }
    }
}