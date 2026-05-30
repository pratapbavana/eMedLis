using eMedLis.Models.ReportAuthorization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.DAL.ReportAuthorization
{
    public class ReportAuthorizationDB
    {
        private readonly string _connectionString;

        public ReportAuthorizationDB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;
        }

        public List<ReportAuthorizationListItem> SearchPending(string userName, DateTime? dateFrom, DateTime? dateTo, string patientName, string sampleBarcode, string investigation, bool criticalOnly)
        {
            var list = new List<ReportAuthorizationListItem>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportAuthorization", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", (object)(userName ?? string.Empty));
                cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = (object)dateFrom ?? DBNull.Value;
                cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = (object)dateTo ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PatientName", (object)(patientName ?? string.Empty));
                cmd.Parameters.AddWithValue("@SampleBarcode", (object)(sampleBarcode ?? string.Empty));
                cmd.Parameters.AddWithValue("@Investigation", (object)(investigation ?? string.Empty));
                cmd.Parameters.AddWithValue("@CriticalOnly", criticalOnly);
                cmd.Parameters.AddWithValue("@Action", "SearchPending");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new ReportAuthorizationListItem
                        {
                            SampleDetailId = Convert.ToInt32(rdr["SampleDetailId"]),
                            SampleCollectionId = Convert.ToInt32(rdr["SampleCollectionId"]),
                            SampleBarcode = Convert.ToString(rdr["SampleBarcode"]),
                            PatientName = Convert.ToString(rdr["PatName"]),
                            PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]),
                            PatientAgeType = Convert.ToString(rdr["AgeType"]),
                            PatientGender = Convert.ToString(rdr["Gender"]),
                            InvestigationName = Convert.ToString(rdr["InvestigationName"]),
                            DepartmentName = Convert.ToString(rdr["DepartmentName"]),
                            CollectionDate = rdr["CollectionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["CollectionDate"]),
                            ResultStatus = Convert.ToString(rdr["ResultStatus"]),
                            HasCritical = rdr["HasCritical"] != DBNull.Value && Convert.ToBoolean(rdr["HasCritical"])
                        });
                    }
                }
            }

            return list;
        }

        public ReportAuthorizationReviewResponse GetReview(string userName, int sampleDetailId)
        {
            var response = new ReportAuthorizationReviewResponse
            {
                Results = new List<ReportAuthorizationResultItem>(),
                Patient = new ReportAuthorizationPatientInfo()
            };

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportAuthorization", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", (object)(userName ?? string.Empty));
                cmd.Parameters.AddWithValue("@SampleDetailId", sampleDetailId);
                cmd.Parameters.AddWithValue("@Action", "GetReview");

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        return null;
                    }

                    response.SampleDetailId = sampleDetailId;
                    response.ResultStatus = Convert.ToString(rdr["ResultStatus"]);
                    response.Interpretation = Convert.ToString(rdr["DoctorInterpretation"]);
                    response.RejectedReason = Convert.ToString(rdr["RejectedReason"]);
                    response.AuthorizedDoctor = Convert.ToString(rdr["AuthorizedDoctor"]);
                    response.AuthorizedOn = rdr["AuthorizedOn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["AuthorizedOn"]);
                    response.CanAuthorize = rdr["CanAuthorize"] != DBNull.Value && Convert.ToBoolean(rdr["CanAuthorize"]);
                    response.HasSignature = rdr["HasSignature"] != DBNull.Value && Convert.ToBoolean(rdr["HasSignature"]);
                    response.DoctorId = rdr["DoctorId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["DoctorId"]);

                    response.Patient.BillNo = Convert.ToString(rdr["BillNo"]);
                    response.Patient.PatientName = Convert.ToString(rdr["PatName"]);
                    response.Patient.PatientAge = rdr["Age"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Age"]);
                    response.Patient.PatientAgeType = Convert.ToString(rdr["AgeType"]);
                    response.Patient.PatientGender = Convert.ToString(rdr["Gender"]);
                    response.Patient.SampleBarcode = Convert.ToString(rdr["SampleBarcode"]);
                    response.Patient.InvestigationName = Convert.ToString(rdr["InvestigationName"]);
                    response.Patient.DepartmentName = Convert.ToString(rdr["DepartmentName"]);

                    if (rdr.NextResult())
                    {
                        while (rdr.Read())
                        {
                            response.Results.Add(new ReportAuthorizationResultItem
                            {
                                ParameterId = Convert.ToInt32(rdr["ParameterId"]),
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

            return response;
        }

        public Tuple<int, string> SaveReview(string userName, ReportAuthorizationActionRequest request)
        {
            return ExecuteAction(userName, request, "SaveReview");
        }

        public Tuple<int, string> Authorize(string userName, ReportAuthorizationActionRequest request)
        {
            return ExecuteAction(userName, request, "Authorize");
        }

        public Tuple<int, string> Reject(string userName, ReportAuthorizationActionRequest request)
        {
            return ExecuteAction(userName, request, "Reject");
        }

        private Tuple<int, string> ExecuteAction(string userName, ReportAuthorizationActionRequest request, string action)
        {
            int statusCode = 0;
            string statusMsg = "Failed";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportAuthorization", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", (object)(userName ?? string.Empty));
                cmd.Parameters.AddWithValue("@SampleDetailId", request.SampleDetailId);
                cmd.Parameters.AddWithValue("@DoctorInterpretation", (object)(request.Interpretation ?? string.Empty));
                cmd.Parameters.AddWithValue("@RejectedReason", (object)(request.RejectReason ?? string.Empty));
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 300).Direction = ParameterDirection.Output;

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    statusCode = cmd.Parameters["@StatusCode"].Value == DBNull.Value ? 0 : Convert.ToInt32(cmd.Parameters["@StatusCode"].Value);
                    statusMsg = Convert.ToString(cmd.Parameters["@StatusMsg"].Value);
                }
                catch (Exception ex)
                {
                    statusCode = 0;
                    statusMsg = ex.Message;
                }
            }

            return new Tuple<int, string>(statusCode, statusMsg);
        }
    }
}
