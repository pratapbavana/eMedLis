using eMedLis.Models.ReportEntry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace eMedLis.DAL.ReportEntry
{
    public class ReportEntryDB
    {
        private readonly string _connectionString;

        public ReportEntryDB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;
        }

        public List<ReportEntrySearchResult> SearchOrders(string billNo, string sampleBarcode, string patientName, string mobileNo, DateTime? dateFrom, DateTime? dateTo)
        {
            var results = new List<ReportEntrySearchResult>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportEntry", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillNo", (object)(billNo ?? string.Empty));
                cmd.Parameters.AddWithValue("@SampleBarcode", (object)(sampleBarcode ?? string.Empty));
                cmd.Parameters.AddWithValue("@PatientName", (object)(patientName ?? string.Empty));
                cmd.Parameters.AddWithValue("@MobileNo", (object)(mobileNo ?? string.Empty));
                cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = (object)dateFrom ?? DBNull.Value;
                cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = (object)dateTo ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@Action", "SearchOrders");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        results.Add(new ReportEntrySearchResult
                        {
                            SampleCollectionId = Convert.ToInt32(rdr["SampleCollectionId"]),
                            CollectionBarcode = Convert.ToString(rdr["CollectionBarcode"]),
                            CollectionDate = rdr["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CollectionDate"]),
                            BillSummaryId = Convert.ToInt32(rdr["BillSummaryId"]),
                            BillNo = Convert.ToString(rdr["BillNo"]),
                            BillDate = rdr["BillDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["BillDate"]),
                            PatientInfoId = Convert.ToInt32(rdr["PatientInfoId"]),
                            PatientName = Convert.ToString(rdr["PatName"]),
                            PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]),
                            PatientAgeType = Convert.ToString(rdr["AgeType"]),
                            PatientGender = Convert.ToString(rdr["Gender"]),
                            MobileNo = Convert.ToString(rdr["MobileNo"]),
                            UHID = Convert.ToString(rdr["UHID"]),
                            InvestigationCount = Convert.ToInt32(rdr["InvestigationCount"])
                        });
                    }
                }
            }

            return results;
        }

        public List<ReportEntryInvestigationItem> GetInvestigations(int sampleCollectionId)
        {
            var list = new List<ReportEntryInvestigationItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportEntry", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);
                cmd.Parameters.AddWithValue("@Action", "GetInvestigations");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new ReportEntryInvestigationItem
                        {
                            SampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]),
                            InvMasterId = Convert.ToInt32(rdr["InvMasterId"]),
                            InvestigationName = Convert.ToString(rdr["InvestigationName"]),
                            SampleBarcode = Convert.ToString(rdr["SampleBarcode"]),
                            SpecimenType = Convert.ToString(rdr["SpecimenType"]),
                            HasTemplate = Convert.ToBoolean(rdr["HasTemplate"])
                        });
                    }
                }
            }

            return list;
        }

        public ReportEntrySearchResult GetOrderSummary(int sampleCollectionId)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportEntry", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SampleCollectionId", sampleCollectionId);
                cmd.Parameters.AddWithValue("@Action", "GetOrderSummary");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        return null;
                    }

                    return new ReportEntrySearchResult
                    {
                        SampleCollectionId = Convert.ToInt32(rdr["SampleCollectionId"]),
                        CollectionBarcode = Convert.ToString(rdr["CollectionBarcode"]),
                        CollectionDate = rdr["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CollectionDate"]),
                        BillSummaryId = Convert.ToInt32(rdr["BillSummaryId"]),
                        BillNo = Convert.ToString(rdr["BillNo"]),
                        BillDate = rdr["BillDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["BillDate"]),
                        PatientInfoId = Convert.ToInt32(rdr["PatientInfoId"]),
                        PatientName = Convert.ToString(rdr["PatName"]),
                        PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]),
                        PatientAgeType = Convert.ToString(rdr["AgeType"]),
                        PatientGender = Convert.ToString(rdr["Gender"]),
                        MobileNo = Convert.ToString(rdr["MobileNo"]),
                        UHID = Convert.ToString(rdr["UHID"]),
                        InvestigationCount = Convert.ToInt32(rdr["InvestigationCount"])
                    };
                }
            }
        }

        public ReportEntryTemplateResponse LoadTemplate(int sampleDetailId)
        {
            var response = new ReportEntryTemplateResponse
            {
                Patient = GetPatientContext(sampleDetailId),
                TemplateItems = new List<ReportEntryTemplateItem>(),
                Methods = new List<ReportEntryMethodItem>(),
                ResultStatus = "Draft",
                IsEditable = true,
                SavedResults = new List<ReportEntrySavedResultItem>()
            };

            if (response.Patient == null)
            {
                return response;
            }

            response.TemplateItems = GetTemplateItems(response.Patient.InvestigationId);
            response.Methods = GetMethods();
            var header = GetResultHeader(sampleDetailId);
            if (header != null && !string.IsNullOrWhiteSpace(header.ResultStatus))
            {
                response.ResultStatus = header.ResultStatus;
                response.IsEditable =
                    string.Equals(header.ResultStatus, "Draft", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(header.ResultStatus, "Rejected", StringComparison.OrdinalIgnoreCase);
            }
            response.SavedResults = GetSavedResults(sampleDetailId);

            return response;
        }

        public Tuple<int, string> SaveResults(ReportEntrySaveRequest request, string targetStatus)
        {
            if (request == null || request.SampleDetailId <= 0)
            {
                return new Tuple<int, string>(0, "Invalid save request");
            }

            var existing = GetResultHeader(request.SampleDetailId);
            if (existing != null && !string.IsNullOrWhiteSpace(existing.ResultStatus)
                && !string.Equals(existing.ResultStatus, "Draft", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(existing.ResultStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return new Tuple<int, string>(0, "Results cannot be modified in current status");
            }

            var items = request.Items ?? new List<ReportEntrySaveItem>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        var headerId = UpsertResultHeader(con, tran, request.SampleDetailId, targetStatus);
                        DeleteResultDetails(con, tran, headerId);

                        foreach (var item in items)
                        {
                            AddResultDetail(con, tran, headerId, item);
                        }

                        tran.Commit();
                        return new Tuple<int, string>(1, targetStatus == "Pending Authorization"
                            ? "Results submitted for authorization"
                            : "Results saved as draft");
                    }
                    catch
                    {
                        tran.Rollback();
                        return new Tuple<int, string>(0, "Failed to save results");
                    }
                }
            }
        }

        public List<ReportEntryRangeItem> LoadRanges(int sampleDetailId, int methodId)
        {
            var patient = GetPatientContext(sampleDetailId);
            if (patient == null)
            {
                return new List<ReportEntryRangeItem>();
            }

            var parameterIds = GetTemplateItems(patient.InvestigationId)
                .Where(x => string.Equals(x.ItemType, "Parameter", StringComparison.OrdinalIgnoreCase) && x.ParameterId.HasValue)
                .Select(x => x.ParameterId.Value)
                .Distinct()
                .ToList();

            var result = new List<ReportEntryRangeItem>();
            foreach (var parameterId in parameterIds)
            {
                var matched = FindBestRange(parameterId, methodId, patient.AgeInDays, patient.PatientGender);
                result.Add(matched ?? new ReportEntryRangeItem
                {
                    ParameterId = parameterId,
                    DisplayRange = "-",
                    Found = false
                });
            }

            return result;
        }

        public ReportEntryRangeItem LoadParameterRange(int sampleDetailId, int parameterId, int methodId)
        {
            var patient = GetPatientContext(sampleDetailId);
            if (patient == null)
            {
                return new ReportEntryRangeItem
                {
                    ParameterId = parameterId,
                    DisplayRange = "-",
                    Found = false
                };
            }

            var matched = FindBestRange(parameterId, methodId, patient.AgeInDays, patient.PatientGender);
            return matched ?? new ReportEntryRangeItem
            {
                ParameterId = parameterId,
                DisplayRange = "-",
                Found = false
            };
        }

        private ReportEntryPatientContext GetPatientContext(int sampleDetailId)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportEntry", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SampleDetailId", sampleDetailId);
                cmd.Parameters.AddWithValue("@Action", "GetPatientContext");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        return null;
                    }

                    var age = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]);
                    var ageType = Convert.ToString(rdr["AgeType"]);

                    return new ReportEntryPatientContext
                    {
                        SampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]),
                        InvestigationId = Convert.ToInt32(rdr["InvMasterId"]),
                        InvestigationName = Convert.ToString(rdr["InvestigationName"]),
                        PatientName = Convert.ToString(rdr["PatName"]),
                        PatientAge = age,
                        PatientAgeType = ageType,
                        PatientGender = Convert.ToString(rdr["Gender"]),
                        SampleBarcode = Convert.ToString(rdr["SampleBarcode"]),
                        BillNo = Convert.ToString(rdr["BillNo"]),
                        AgeInDays = ConvertAgeToDays(age, ageType)
                    };
                }
            }
        }

        private List<ReportEntryTemplateItem> GetTemplateItems(int investigationId)
        {
            var list = new List<ReportEntryTemplateItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_InvestigationTemplate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InvestigationId", investigationId.ToString(CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@Action", "GetByInvestigation");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new ReportEntryTemplateItem
                        {
                            ItemType = Convert.ToString(rdr["ItemType"]),
                            HeaderId = rdr["HeaderId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["HeaderId"]),
                            HeaderName = Convert.ToString(rdr["HeaderName"]),
                            ParameterId = rdr["ParameterId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ParameterId"]),
                            ParameterName = Convert.ToString(rdr["ParameterName"]),
                            DefaultMethodId = HasColumn(rdr, "MethodId") && rdr["MethodId"] != DBNull.Value ? (int?)Convert.ToInt32(rdr["MethodId"]) : null,
                            DefaultMethodName = HasColumn(rdr, "MethodName") ? Convert.ToString(rdr["MethodName"]) : string.Empty,
                            Unit = HasColumn(rdr, "Unit") ? Convert.ToString(rdr["Unit"]) : string.Empty,
                            ResultType = HasColumn(rdr, "ResultType") ? Convert.ToString(rdr["ResultType"]) : "Numeric",
                            Formula = HasColumn(rdr, "Formula") ? Convert.ToString(rdr["Formula"]) : string.Empty,
                            DecimalPrecision = HasColumn(rdr, "DecimalPrecision") && rdr["DecimalPrecision"] != DBNull.Value ? Convert.ToInt32(rdr["DecimalPrecision"]) : 0,
                            DisplayOrder = Convert.ToInt32(rdr["DisplayOrder"])
                        });
                    }
                }
            }

            return list;
        }

        private List<ReportEntryMethodItem> GetMethods()
        {
            var list = new List<ReportEntryMethodItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_TestMethod", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GetMethods");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var active = rdr["Active"] == DBNull.Value || Convert.ToBoolean(rdr["Active"]);
                        if (!active)
                        {
                            continue;
                        }

                        list.Add(new ReportEntryMethodItem
                        {
                            MethodId = Convert.ToInt32(rdr["Id"]),
                            MethodName = Convert.ToString(rdr["MethodName"])
                        });
                    }
                }
            }

            return list.OrderBy(x => x.MethodName == "None" ? 0 : 1).ThenBy(x => x.MethodName).ToList();
        }

        private ReportEntryRangeItem FindBestRange(int parameterId, int methodId, int ageInDays, string patientGender)
        {
            var candidates = new List<RangeCandidate>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReferenceRange", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ParameterId", parameterId);
                cmd.Parameters.AddWithValue("@MethodId", methodId);
                cmd.Parameters.AddWithValue("@Action", "GetByParameterMethod");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        if (!(rdr["Active"] == DBNull.Value ? false : Convert.ToBoolean(rdr["Active"])))
                        {
                            continue;
                        }

                        var gender = Convert.ToString(rdr["Gender"]);
                        var fromDays = rdr["AgeFromDays"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["AgeFromDays"]);
                        var toDays = rdr["AgeToDays"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["AgeToDays"]);
                        var normalMin = rdr["NormalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMin"]);
                        var normalMax = rdr["NormalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMax"]);
                        var criticalMin = rdr["CriticalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMin"]);
                        var criticalMax = rdr["CriticalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMax"]);
                        var rangeText = HasColumn(rdr, "RangeText") ? Convert.ToString(rdr["RangeText"]) : string.Empty;

                        candidates.Add(new RangeCandidate
                        {
                            Gender = gender,
                            FromDays = fromDays,
                            ToDays = toDays,
                            NormalMin = normalMin,
                            NormalMax = normalMax,
                            CriticalMin = criticalMin,
                            CriticalMax = criticalMax,
                            RangeText = rangeText
                        });
                    }
                }
            }

            var filtered = candidates
                .Where(x => ageInDays >= x.FromDays && ageInDays <= x.ToDays)
                .Select(x => new
                {
                    Gender = x.Gender,
                    FromDays = x.FromDays,
                    ToDays = x.ToDays,
                    NormalMin = x.NormalMin,
                    NormalMax = x.NormalMax,
                    CriticalMin = x.CriticalMin,
                    CriticalMax = x.CriticalMax,
                    RangeText = x.RangeText,
                    Priority = GetGenderPriority(x.Gender, patientGender)
                })
                .Where(x => x.Priority > 0)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.FromDays)
                .FirstOrDefault();

            if (filtered == null)
            {
                return null;
            }

            return new ReportEntryRangeItem
            {
                ParameterId = parameterId,
                NormalMin = filtered.NormalMin,
                NormalMax = filtered.NormalMax,
                CriticalMin = filtered.CriticalMin,
                CriticalMax = filtered.CriticalMax,
                RangeText = filtered.RangeText,
                DisplayRange = FormatRange(filtered.NormalMin, filtered.NormalMax, filtered.RangeText),
                Found = true
            };
        }

        private ReportResultHeaderInfo GetResultHeader(int sampleDetailId)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportResult", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SampleDetailId", sampleDetailId);
                cmd.Parameters.AddWithValue("@Action", "GetHeader");
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        return null;
                    }

                    return new ReportResultHeaderInfo
                    {
                        ResultHeaderId = Convert.ToInt32(rdr["Id"]),
                        SampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]),
                        ResultStatus = Convert.ToString(rdr["ResultStatus"])
                    };
                }
            }
        }

        private List<ReportEntrySavedResultItem> GetSavedResults(int sampleDetailId)
        {
            var list = new List<ReportEntrySavedResultItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportResult", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SampleDetailId", sampleDetailId);
                cmd.Parameters.AddWithValue("@Action", "GetDetailsBySampleDetail");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new ReportEntrySavedResultItem
                        {
                            ParameterId = Convert.ToInt32(rdr["ParameterId"]),
                            MethodId = rdr["MethodId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["MethodId"]),
                            ResultValue = Convert.ToString(rdr["ResultValue"]),
                            ResultType = Convert.ToString(rdr["ResultType"]),
                            Unit = Convert.ToString(rdr["Unit"]),
                            NormalMin = rdr["NormalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMin"]),
                            NormalMax = rdr["NormalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMax"]),
                            CriticalMin = rdr["CriticalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMin"]),
                            CriticalMax = rdr["CriticalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMax"]),
                            RangeText = HasColumn(rdr, "RangeText") ? Convert.ToString(rdr["RangeText"]) : string.Empty,
                            Flag = Convert.ToString(rdr["Flag"]),
                            IsCritical = rdr["IsCritical"] != DBNull.Value && Convert.ToBoolean(rdr["IsCritical"]),
                            DisplayOrder = rdr["DisplayOrder"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["DisplayOrder"])
                        });
                    }
                }
            }

            return list;
        }

        private static int UpsertResultHeader(SqlConnection con, SqlTransaction tran, int sampleDetailId, string targetStatus)
        {
            using (var cmd = new SqlCommand("Usp_ReportResult", con, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SampleDetailId", sampleDetailId);
                cmd.Parameters.AddWithValue("@ResultStatus", targetStatus);
                cmd.Parameters.AddWithValue("@Action", "UpsertHeader");
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return Convert.ToInt32(rdr["ResultHeaderId"]);
                    }
                }
            }

            throw new InvalidOperationException("Unable to create result header");
        }

        private static void DeleteResultDetails(SqlConnection con, SqlTransaction tran, int headerId)
        {
            using (var cmd = new SqlCommand("Usp_ReportResult", con, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ResultHeaderId", headerId);
                cmd.Parameters.AddWithValue("@Action", "DeleteDetails");
                cmd.ExecuteNonQuery();
            }
        }

        private static void AddResultDetail(SqlConnection con, SqlTransaction tran, int headerId, ReportEntrySaveItem item)
        {
            using (var cmd = new SqlCommand("Usp_ReportResult", con, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ResultHeaderId", headerId);
                cmd.Parameters.AddWithValue("@ParameterId", item.ParameterId);
                cmd.Parameters.AddWithValue("@MethodId", (object)item.MethodId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ResultValue", (object)item.ResultValue ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ResultType", (object)item.ResultType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Unit", (object)item.Unit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NormalMin", (object)item.NormalMin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NormalMax", (object)item.NormalMax ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CriticalMin", (object)item.CriticalMin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CriticalMax", (object)item.CriticalMax ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RangeText", (object)item.RangeText ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Flag", (object)item.Flag ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsCritical", item.IsCritical);
                cmd.Parameters.AddWithValue("@DisplayOrder", item.DisplayOrder);
                cmd.Parameters.AddWithValue("@Action", "AddDetail");
                cmd.ExecuteNonQuery();
            }
        }

        private static int ConvertAgeToDays(int age, string ageType)
        {
            var normalized = (ageType ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "1" || normalized.Contains("year"))
            {
                return age * 365;
            }

            if (normalized == "2" || normalized.Contains("month"))
            {
                return age * 30;
            }

            if (normalized == "3" || normalized.Contains("day"))
            {
                return age;
            }

            return age * 365;
        }

        private static int GetGenderPriority(string rangeGender, string patientGender)
        {
            var rg = NormalizeGender(rangeGender);
            var pg = NormalizeGender(patientGender);

            if (string.IsNullOrWhiteSpace(rg) || rg == "all")
            {
                return 1;
            }

            if (rg == pg)
            {
                return 2;
            }

            return 0;
        }

        private static string NormalizeGender(string raw)
        {
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(value))
            {
                return "all";
            }

            if (value == "1" || value == "m" || value == "male")
            {
                return "male";
            }

            if (value == "2" || value == "f" || value == "female")
            {
                return "female";
            }

            if (value == "all" || value == "both" || value == "any")
            {
                return "all";
            }

            return value;
        }

        private static string FormatRange(decimal? normalMin, decimal? normalMax, string rangeText)
        {
            if (!string.IsNullOrWhiteSpace(rangeText))
            {
                return rangeText;
            }

            if (normalMin.HasValue && normalMax.HasValue)
            {
                return normalMin.Value.ToString("0.####", CultureInfo.InvariantCulture) + " - " +
                       normalMax.Value.ToString("0.####", CultureInfo.InvariantCulture);
            }

            return "-";
        }

        private static bool HasColumn(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private class ReportResultHeaderInfo
        {
            public int ResultHeaderId { get; set; }
            public int SampleDetailId { get; set; }
            public string ResultStatus { get; set; }
        }

        private class RangeCandidate
        {
            public string Gender { get; set; }
            public int FromDays { get; set; }
            public int ToDays { get; set; }
            public decimal? NormalMin { get; set; }
            public decimal? NormalMax { get; set; }
            public decimal? CriticalMin { get; set; }
            public decimal? CriticalMax { get; set; }
            public string RangeText { get; set; }
        }
    }
}
