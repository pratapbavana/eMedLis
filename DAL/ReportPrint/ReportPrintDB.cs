using eMedLis.Models.ReportPrint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace eMedLis.DAL.ReportPrint
{
    public class ReportPrintDB
    {
        private readonly string _connectionString;

        public ReportPrintDB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;
        }

        public List<ReportPrintBillItem> SearchAuthorizedBills(string patientName, string mobileNo, string sampleBarcode, string billNo, DateTime? dateFrom, DateTime? dateTo, string subDepartment)
        {
            var list = new List<ReportPrintBillItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportPrint", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientName", (object)(patientName ?? string.Empty));
                cmd.Parameters.AddWithValue("@MobileNo", (object)(mobileNo ?? string.Empty));
                cmd.Parameters.AddWithValue("@SampleBarcode", (object)(sampleBarcode ?? string.Empty));
                cmd.Parameters.AddWithValue("@BillNo", (object)(billNo ?? string.Empty));
                cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = (object)dateFrom ?? DBNull.Value;
                cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = (object)dateTo ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@SubDepartment", (object)(subDepartment ?? string.Empty));
                cmd.Parameters.AddWithValue("@Action", "SearchBills");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new ReportPrintBillItem
                        {
                            BillSummaryId = Convert.ToInt32(rdr["BillSummaryId"]),
                            SampleCollectionId = Convert.ToInt32(rdr["SampleCollectionId"]),
                            BillNo = Convert.ToString(rdr["BillNo"]),
                            CollectionBarcode = Convert.ToString(rdr["CollectionBarcode"]),
                            PatientName = Convert.ToString(rdr["PatName"]),
                            PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]),
                            PatientAgeType = Convert.ToString(rdr["AgeType"]),
                            PatientGender = Convert.ToString(rdr["Gender"]),
                            MobileNo = Convert.ToString(rdr["MobileNo"]),
                            CollectionDate = rdr["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CollectionDate"]),
                            InvestigationCount = rdr["InvestigationCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["InvestigationCount"])
                        });
                    }
                }
            }

            return list;
        }

        public List<ReportPrintSearchItem> GetBillInvestigations(int billSummaryId)
        {
            var list = new List<ReportPrintSearchItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportPrint", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                cmd.Parameters.AddWithValue("@Action", "GetBillInvestigations");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(MapItem(rdr));
                    }
                }
            }

            return list;
        }

        public List<ReportPrintSearchItem> GetSelectedAuthorized(int billSummaryId, List<int> sampleDetailIds)
        {
            var ids = sampleDetailIds ?? new List<int>();
            ids = ids.Where(x => x > 0).Distinct().ToList();
            if (!ids.Any())
            {
                return new List<ReportPrintSearchItem>();
            }

            var list = new List<ReportPrintSearchItem>();
            var idCsv = string.Join(",", ids);

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportPrint", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                cmd.Parameters.AddWithValue("@SampleDetailIds", idCsv);
                cmd.Parameters.AddWithValue("@Action", "GetByIds");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(MapItem(rdr));
                    }
                }
            }

            return list;
        }

        public List<ReportPreviewDocument> GetPreviewDocuments(int billSummaryId, List<int> sampleDetailIds)
        {
            var ids = sampleDetailIds ?? new List<int>();
            ids = ids.Where(x => x > 0).Distinct().ToList();
            if (!ids.Any())
            {
                return new List<ReportPreviewDocument>();
            }

            var documents = new List<ReportPreviewDocument>();
            var map = new Dictionary<int, ReportPreviewDocument>();
            var idCsv = string.Join(",", ids);

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportPrint", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                cmd.Parameters.AddWithValue("@SampleDetailIds", idCsv);
                cmd.Parameters.AddWithValue("@Action", "GetPreviewData");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var doc = new ReportPreviewDocument
                        {
                            SampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]),
                            BillSummaryId = Convert.ToInt32(rdr["BillSummaryId"]),
                            BillNo = Convert.ToString(rdr["BillNo"]),
                            BillDate = ReadNullableDateTime(rdr, "BillDate"),
                            SampleBarcode = Convert.ToString(rdr["SampleBarcode"]),
                            PatientName = Convert.ToString(rdr["PatName"]),
                            PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]),
                            PatientAgeType = Convert.ToString(rdr["AgeType"]),
                            PatientGender = Convert.ToString(rdr["Gender"]),
                            MobileNo = Convert.ToString(rdr["MobileNo"]),
                            ReferralDoctor = ReadString(rdr, "ReferralDoctor"),
                            InvestigationName = Convert.ToString(rdr["InvestigationName"]),
                            DepartmentName = Convert.ToString(rdr["DepartmentName"]),
                            CollectionDate = rdr["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CollectionDate"]),
                            ResultStatus = Convert.ToString(rdr["ResultStatus"]),
                            DoctorInterpretation = Convert.ToString(rdr["DoctorInterpretation"]),
                            AuthorizedOn = rdr["AuthorizedOn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["AuthorizedOn"]),
                            AuthorizedDoctor = Convert.ToString(rdr["AuthorizedDoctor"]),
                            AuthorizedDoctorId = ReadNullableInt(rdr, "AuthorizedDoctorId") ?? ReadNullableInt(rdr, "AuthorizedByDoctorId"),
                            HasSignature = rdr["HasSignature"] != DBNull.Value && Convert.ToBoolean(rdr["HasSignature"]),
                            Parameters = new List<ReportPreviewParameterItem>()
                        };

                        documents.Add(doc);
                        map[doc.SampleDetailId] = doc;
                    }

                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            var sampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]);
                            ReportPreviewDocument parent;
                            if (!map.TryGetValue(sampleDetailId, out parent))
                            {
                                continue;
                            }

                            parent.Parameters.Add(new ReportPreviewParameterItem
                            {
                                SampleDetailId = sampleDetailId,
                                HeaderName = Convert.ToString(rdr["HeaderName"]),
                                ParameterName = Convert.ToString(rdr["ParameterName"]),
                                MethodName = Convert.ToString(rdr["MethodName"]),
                                ResultValue = Convert.ToString(rdr["ResultValue"]),
                                Unit = Convert.ToString(rdr["Unit"]),
                                NormalRange = Convert.ToString(rdr["NormalRange"]),
                                Flag = Convert.ToString(rdr["Flag"]),
                                IsCritical = rdr["IsCritical"] != DBNull.Value && Convert.ToBoolean(rdr["IsCritical"]),
                                DisplayOrder = rdr["DisplayOrder"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["DisplayOrder"])
                            });
                        }
                    }
                }
            }

            FillBillContext(billSummaryId, documents);
            return documents;
        }

        private void FillBillContext(int billSummaryId, List<ReportPreviewDocument> documents)
        {
            if (documents == null || documents.Count == 0)
            {
                return;
            }

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT BS.BillDate, PI.Ref AS ReferralDoctor
                FROM BillSummary BS
                INNER JOIN PatientInfo PI ON PI.PatientInfoId = BS.PatientInfoId
                WHERE BS.BillSummaryId = @BillSummaryId", con))
            {
                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        return;
                    }

                    var billDate = rdr["BillDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["BillDate"]);
                    var referralDoctor = Convert.ToString(rdr["ReferralDoctor"]);
                    foreach (var doc in documents)
                    {
                        if (!doc.BillDate.HasValue)
                        {
                            doc.BillDate = billDate;
                        }
                        if (string.IsNullOrWhiteSpace(doc.ReferralDoctor))
                        {
                            doc.ReferralDoctor = referralDoctor;
                        }
                    }
                }
            }
        }

        private static string ReadString(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return reader[i] == DBNull.Value ? string.Empty : Convert.ToString(reader[i]);
                }
            }
            return string.Empty;
        }

        private static DateTime? ReadNullableDateTime(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return reader[i] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[i]);
                }
            }
            return null;
        }

        private static int? ReadNullableInt(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return reader[i] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[i]);
                }
            }
            return null;
        }

        private static ReportPrintSearchItem MapItem(IDataRecord rdr)
        {
            return new ReportPrintSearchItem
            {
                SampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]),
                SampleCollectionId = Convert.ToInt32(rdr["SampleCollectionId"]),
                BillSummaryId = Convert.ToInt32(rdr["BillSummaryId"]),
                SampleBarcode = Convert.ToString(rdr["SampleBarcode"]),
                BillNo = Convert.ToString(rdr["BillNo"]),
                PatientName = Convert.ToString(rdr["PatName"]),
                PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]),
                PatientAgeType = Convert.ToString(rdr["AgeType"]),
                PatientGender = Convert.ToString(rdr["Gender"]),
                MobileNo = Convert.ToString(rdr["MobileNo"]),
                InvestigationName = Convert.ToString(rdr["InvestigationName"]),
                DepartmentName = Convert.ToString(rdr["DepartmentName"]),
                CollectionDate = rdr["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CollectionDate"]),
                ResultStatus = Convert.ToString(rdr["ResultStatus"])
            };
        }
    }
}
