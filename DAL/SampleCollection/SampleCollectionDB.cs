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

        #region Helper Methods for Safe Data Reading

        /// <summary>
        /// Safely read any value from SqlDataReader, returning default if null
        /// </summary>
        private object SafeGetValue(SqlDataReader reader, string columnName, object defaultValue = null)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetValue(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read integer from SqlDataReader
        /// </summary>
        private int SafeGetInt(SqlDataReader reader, string columnName, int defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading int column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read string from SqlDataReader
        /// </summary>
        private string SafeGetString(SqlDataReader reader, string columnName, string defaultValue = "")
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading string column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read DateTime from SqlDataReader
        /// </summary>
        private DateTime SafeGetDateTime(SqlDataReader reader, string columnName, DateTime? defaultValue = null)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? (defaultValue ?? DateTime.Now) : reader.GetDateTime(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading datetime column {columnName}: {ex.Message}");
                return defaultValue ?? DateTime.Now;
            }
        }

        /// <summary>
        /// Safely read TimeSpan from SqlDataReader
        /// </summary>
        private TimeSpan SafeGetTimeSpan(SqlDataReader reader, string columnName, TimeSpan? defaultValue = null)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return defaultValue ?? TimeSpan.FromHours(9);

                var value = reader.GetValue(ordinal);
                if (value is TimeSpan ts)
                    return ts;
                if (value is string str && TimeSpan.TryParse(str, out var parsedTs))
                    return parsedTs;

                return defaultValue ?? TimeSpan.FromHours(9);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading timespan column {columnName}: {ex.Message}");
                return defaultValue ?? TimeSpan.FromHours(9);
            }
        }

        /// <summary>
        /// Safely read decimal from SqlDataReader
        /// </summary>
        private decimal SafeGetDecimal(SqlDataReader reader, string columnName, decimal defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetDecimal(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading decimal column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read boolean from SqlDataReader
        /// </summary>
        private bool SafeGetBoolean(SqlDataReader reader, string columnName, bool defaultValue = false)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetBoolean(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading boolean column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read double from SqlDataReader
        /// </summary>
        private double SafeGetDouble(SqlDataReader reader, string columnName, double defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetDouble(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading double column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read long from SqlDataReader
        /// </summary>
        private long SafeGetLong(SqlDataReader reader, string columnName, long defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt64(ordinal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading long column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely read byte array from SqlDataReader
        /// </summary>
        private byte[] SafeGetBytes(SqlDataReader reader, string columnName, byte[] defaultValue = null)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return defaultValue;

                var value = reader.GetValue(ordinal);
                return value as byte[] ?? defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading bytes column {columnName}: {ex.Message}");
                return defaultValue;
            }
        }

        #endregion

        #region Data Access Methods

        public SampleCollectionViewModel GetBillForCollection(int billSummaryId)
        {
            var viewModel = new SampleCollectionViewModel();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetBillForSampleCollection", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        var billDetails = new List<BillDetail>();

                        while (reader.Read())
                        {
                            if (viewModel.PatientInfo == null)
                            {
                                viewModel.PatientInfo = new PatientInfo
                                {
                                    PatientInfoId = SafeGetInt(reader, "PatientInfoId"),
                                    UHID = SafeGetString(reader, "UHID"),
                                    PatName = SafeGetString(reader, "PatName"),
                                    MobileNo = SafeGetString(reader, "MobileNo"),
                                    Age = SafeGetInt(reader, "Age"),
                                    AgeType = SafeGetString(reader, "AgeType"),
                                    Gender = SafeGetString(reader, "Gender"),
                                    Area = SafeGetString(reader, "Area"),
                                    City = SafeGetString(reader, "City"),
                                    Ref = SafeGetString(reader, "Ref")
                                };

                                viewModel.BillSummary = new BillSummary
                                {
                                    BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                    BillNo = SafeGetString(reader, "BillNo"),
                                    BillDate = SafeGetDateTime(reader, "BillDate"),
                                    NetAmount = SafeGetDecimal(reader, "NetAmount"),
                                    TotalBill = SafeGetDecimal(reader, "TotalBill"),
                                    TotalDiscountAmount = SafeGetDecimal(reader, "TotalDiscountAmount"),
                                    PaidAmount = SafeGetDecimal(reader, "PaidAmount"),
                                    DueAmount = SafeGetDecimal(reader, "DueAmount")
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("InvId")))
                            {
                                billDetails.Add(new BillDetail
                                {
                                    InvId = SafeGetString(reader, "InvId"),
                                    InvName = SafeGetString(reader, "InvName"),
                                    Rate = SafeGetDecimal(reader, "Rate"),
                                    SpecimenType = SafeGetString(reader, "SpecimenName", "Serum"),
                                    ContainerType = SafeGetString(reader, "VacutainerName", "Plain Vacutainer"),
                                    FastingRequired = SafeGetBoolean(reader, "FastingRequired"),
                                    SpecialInstructions = SafeGetString(reader, "SpecialInstructions")
                                });
                            }
                        }

                        viewModel.BillDetails = billDetails;
                    }
                }
            }

            return viewModel;
        }

        public List<ContainerMaster> GetActiveContainers()
        {
            var containers = new List<ContainerMaster>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetActiveContainers", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            containers.Add(new ContainerMaster
                            {
                                ContainerId = SafeGetInt(reader, "ContainerId"),
                                ContainerName = SafeGetString(reader, "ContainerName"),
                                ContainerCode = SafeGetString(reader, "ContainerCode"),
                                CapColor = SafeGetString(reader, "CapColor"),
                                Volume = SafeGetString(reader, "Volume"),
                                Additive = SafeGetString(reader, "Additive"),
                                StorageTemp = SafeGetString(reader, "StorageTemp"),
                                ExpiryDays = SafeGetInt(reader, "ExpiryDays"),
                                Active = SafeGetBoolean(reader, "Active")
                            });
                        }
                    }
                }
            }

            return containers;
        }

        public List<SampleCollectionViewModel> GetPendingCollectionsWithSummary()
        {
            var collections = new List<SampleCollectionViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetPendingCollectionsWithSummary", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                collections.Add(new SampleCollectionViewModel
                                {
                                    SampleCollection = new SampleCollectionModel
                                    {
                                        SampleCollectionId = SafeGetInt(reader, "SampleCollectionId", 0),
                                        BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                        CollectionBarcode = SafeGetString(reader, "CollectionBarcode", ""),
                                        CollectionDate = SafeGetDateTime(reader, "CollectionDate"),
                                        CollectionTime = SafeGetTimeSpan(reader, "CollectionTime"),
                                        CollectionStatus = SafeGetString(reader, "CollectionStatus", "New"),
                                        Priority = SafeGetString(reader, "Priority", "Normal"),
                                        HomeCollection = SafeGetBoolean(reader, "HomeCollection", false),
                                        CollectedBy = SafeGetString(reader, "CollectedBy", "")
                                    },
                                    PatientInfo = new PatientInfo
                                    {
                                        PatientInfoId = SafeGetInt(reader, "PatientInfoId"),
                                        PatName = SafeGetString(reader, "PatName"),
                                        UHID = SafeGetString(reader, "UHID"),
                                        MobileNo = SafeGetString(reader, "MobileNo"),
                                        Age = SafeGetInt(reader, "Age"),
                                        Gender = SafeGetString(reader, "Gender"),
                                        Area = SafeGetString(reader, "Area"),
                                        City = SafeGetString(reader, "City")
                                    },
                                    BillSummary = new BillSummary
                                    {
                                        BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                        BillNo = SafeGetString(reader, "BillNo"),
                                        BillDate = SafeGetDateTime(reader, "BillDate"),
                                        NetAmount = SafeGetDecimal(reader, "NetAmount")
                                    },
                                    TotalInvestigations = SafeGetInt(reader, "TotalInvestigations"),
                                    CollectedCount = SafeGetInt(reader, "CollectedCount"),
                                    PendingCount = SafeGetInt(reader, "PendingCount")
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error mapping row: {ex.Message}");
                                continue;
                            }
                        }
                    }
                }
            }

            return collections;
        }

        public SampleCollectionViewModel GetSampleCollectionDetailsForEdit(int sampleCollectionId)
        {
            var viewModel = new SampleCollectionViewModel();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetSampleCollectionDetailsForEdit", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Read master
                        if (reader.Read())
                        {
                            viewModel.SampleCollection = new SampleCollectionModel
                            {
                                SampleCollectionId = SafeGetInt(reader, "SampleCollectionId"),
                                BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                CollectionBarcode = SafeGetString(reader, "CollectionBarcode"),
                                CollectionDate = SafeGetDateTime(reader, "CollectionDate"),
                                CollectionTime = SafeGetTimeSpan(reader, "CollectionTime"),
                                CollectedBy = SafeGetString(reader, "CollectedBy"),
                                CollectionStatus = SafeGetString(reader, "CollectionStatus"),
                                Priority = SafeGetString(reader, "Priority"),
                                Remarks = SafeGetString(reader, "Remarks"),
                                HomeCollection = SafeGetBoolean(reader, "HomeCollection"),
                                PatientAddress = SafeGetString(reader, "PatientAddress")
                            };

                            viewModel.PatientInfo = new PatientInfo
                            {
                                PatientInfoId = SafeGetInt(reader, "PatientInfoId"),
                                PatName = SafeGetString(reader, "PatName"),
                                UHID = SafeGetString(reader, "UHID"),
                                MobileNo = SafeGetString(reader, "MobileNo"),
                                Age = SafeGetInt(reader, "Age"),
                                Gender = SafeGetString(reader, "Gender")
                            };

                            viewModel.BillSummary = new BillSummary
                            {
                                BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                BillNo = SafeGetString(reader, "BillNo"),
                                NetAmount = SafeGetDecimal(reader, "NetAmount")
                            };
                        }

                        // Read details
                        var sampleDetails = new List<SampleCollectionDetail>();
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                sampleDetails.Add(new SampleCollectionDetail
                                {
                                    SampleDetailId = SafeGetInt(reader, "SampleDetailId"),
                                    SampleCollectionId = SafeGetInt(reader, "SampleCollectionId"),
                                    InvMasterId = SafeGetInt(reader, "InvMasterId"),
                                    InvestigationName = SafeGetString(reader, "InvestigationName"),
                                    SpecimenType = SafeGetString(reader, "SpecimenType"),
                                    ContainerType = SafeGetString(reader, "ContainerType"),
                                    SampleBarcode = SafeGetString(reader, "SampleBarcode"),
                                    FastingRequired = SafeGetBoolean(reader, "FastingRequired"),
                                    SpecialInstructions = SafeGetString(reader, "SpecialInstructions"),
                                    SampleStatus = SafeGetString(reader, "SampleStatus"),
                                    CollectedQuantity = SafeGetString(reader, "CollectedQuantity"),
                                    RejectionReason = SafeGetString(reader, "RejectionReason"),
                                    CollectionDate = SafeGetDateTime(reader, "CollectionDate", null),
                                    CollectionTime = SafeGetTimeSpan(reader, "CollectionTime", null),
                                    RejectionDate = SafeGetDateTime(reader, "RejectionDate", null),
                                    IsCollected = SafeGetBoolean(reader, "IsCollected"),
                                    IsRejected = SafeGetBoolean(reader, "IsRejected")
                                });
                            }
                        }

                        viewModel.SampleDetails = sampleDetails;
                    }
                }
            }

            return viewModel;
        }

        public SampleCollectionResult SaveSampleCollection(SampleCollectionModel sampleCollection, List<SampleCollectionDetail> sampleDetails)
        {
            var result = new SampleCollectionResult();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int sampleCollectionId;
                        string collectionBarcode;

                        // Get PatientInfoId from BillSummary
                        int patientInfoId;
                        using (var cmd = new SqlCommand("SELECT PatientInfoId FROM BillSummary WHERE BillSummaryId = @BillId", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BillId", sampleCollection.BillSummaryId);
                            patientInfoId = (int)cmd.ExecuteScalar();
                        }

                        // Save master record
                        using (var cmd = new SqlCommand("usp_SaveSampleCollection", connection, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BillSummaryId", sampleCollection.BillSummaryId);
                            cmd.Parameters.AddWithValue("@PatientInfoId", patientInfoId);
                            cmd.Parameters.AddWithValue("@CollectionDate", sampleCollection.CollectionDate);
                            cmd.Parameters.AddWithValue("@CollectionTime", sampleCollection.CollectionTime);
                            cmd.Parameters.AddWithValue("@CollectedBy", sampleCollection.CollectedBy);
                            cmd.Parameters.AddWithValue("@Priority", sampleCollection.Priority);
                            cmd.Parameters.AddWithValue("@HomeCollection", sampleCollection.HomeCollection);
                            cmd.Parameters.AddWithValue("@PatientAddress", (object)sampleCollection.PatientAddress ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Remarks", (object)sampleCollection.Remarks ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", sampleCollection.CreatedBy);

                            var idParam = new SqlParameter("@SampleCollectionId", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(idParam);

                            var barcodeParam = new SqlParameter("@CollectionBarcode", SqlDbType.VarChar, 30)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(barcodeParam);

                            cmd.ExecuteNonQuery();

                            sampleCollectionId = Convert.ToInt32(idParam.Value);
                            collectionBarcode = barcodeParam.Value.ToString();
                        }

                        // Save detail records
                        foreach (var detail in sampleDetails)
                        {
                            using (var cmd = new SqlCommand("usp_SaveSampleCollectionDetail", connection, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);
                                cmd.Parameters.AddWithValue("@InvMasterId", detail.InvMasterId);
                                cmd.Parameters.AddWithValue("@InvestigationName", detail.InvestigationName);
                                cmd.Parameters.AddWithValue("@SpecimenType", detail.SpecimenType);
                                cmd.Parameters.AddWithValue("@ContainerType", detail.ContainerType);
                                cmd.Parameters.AddWithValue("@FastingRequired", detail.FastingRequired);
                                cmd.Parameters.AddWithValue("@SpecialInstructions", (object)detail.SpecialInstructions ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CollectedQuantity", (object)detail.CollectedQuantity ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@SampleStatus", detail.SampleStatus);

                                var sampleBarcodeParam = new SqlParameter("@SampleBarcode", SqlDbType.VarChar, 30)
                                {
                                    Direction = ParameterDirection.Output
                                };
                                cmd.Parameters.Add(sampleBarcodeParam);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        result.Success = true;
                        result.SampleCollectionId = sampleCollectionId;
                        result.CollectionBarcode = collectionBarcode;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.Success = false;
                        result.Message = ex.Message;
                    }
                }
            }

            return result;
        }

        public List<SampleCollectionViewModel> GetPendingCollections()
        {
            return GetPendingCollectionsWithSummary();
        }

        public SampleCollectionViewModel GetSampleCollectionById(int sampleCollectionId)
        {
            return GetSampleCollectionDetailsForEdit(sampleCollectionId);
        }

        public bool UpdateSampleCollectionStatus(int sampleCollectionId, string status, string updatedBy)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_UpdateSampleCollectionStatus", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);
                    cmd.Parameters.AddWithValue("@CollectionStatus", status);
                    cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                    connection.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateSampleDetailStatus(int sampleDetailId, string sampleStatus, string collectedQuantity,
    DateTime? collectionDate = null, TimeSpan? collectionTime = null, string rejectionReason = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_UpdateSampleDetailStatus", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SampleDetailId", sampleDetailId);
                    cmd.Parameters.AddWithValue("@SampleStatus", sampleStatus);
                    cmd.Parameters.AddWithValue("@CollectedQuantity", (object)collectedQuantity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CollectionDate", (object)collectionDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CollectionTime", (object)collectionTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RejectionReason", (object)rejectionReason ?? DBNull.Value);

                    connection.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CalculateAndUpdateCollectionStatus(int sampleCollectionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_CalculateAndUpdateSampleCollectionStatus", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);

                    connection.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public SampleCollectionModel GetSampleCollectionByBillId(int billSummaryId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(@"
            SELECT TOP 1 
                SampleCollectionId, BillSummaryId, CollectionBarcode, 
                CollectionDate, CollectionTime, CollectionStatus,
                Priority, Remarks, HomeCollection, PatientAddress, CollectedBy
            FROM SampleCollection
            WHERE BillSummaryId = @BillSummaryId
            ORDER BY SampleCollectionId DESC", connection))
                {
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                    connection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SampleCollectionModel
                            {
                                SampleCollectionId = SafeGetInt(reader, "SampleCollectionId"),
                                BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                CollectionBarcode = SafeGetString(reader, "CollectionBarcode"),
                                CollectionDate = SafeGetDateTime(reader, "CollectionDate"),
                                CollectionTime = SafeGetTimeSpan(reader, "CollectionTime"),
                                CollectionStatus = SafeGetString(reader, "CollectionStatus"),
                                Priority = SafeGetString(reader, "Priority"),
                                Remarks = SafeGetString(reader, "Remarks"),
                                HomeCollection = SafeGetBoolean(reader, "HomeCollection"),
                                PatientAddress = SafeGetString(reader, "PatientAddress"),
                                CollectedBy = SafeGetString(reader, "CollectedBy")
                            };
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }

    public class SampleCollectionResult
    {
        public bool Success { get; set; }
        public int SampleCollectionId { get; set; }
        public string CollectionBarcode { get; set; }
        public string Message { get; set; }
    }

    public class InvMasterDetails
    {
        public int InvMasterId { get; set; }
        public string InvName { get; set; }
        public string SpecimenName { get; set; }
        public string VacutainerName { get; set; }
        public bool FastingRequired { get; set; }
        public string SpecialInstructions { get; set; }
    }
}
