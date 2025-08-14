using System;
using System.Collections.Generic;
using System.Configuration; // For ConfigurationManager
using System.Data;
using System.Data.SqlClient;
using eMedLis.Models.PatientBilling; // Namespace for your models

namespace eMedLis.DAL.PatientBilling
{
    public class PatientBillingDB
    {
        private readonly string _connectionString;

        public PatientBillingDB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;
        }

        // --- Helper Methods to call individual Stored Procedures ---

        private int SavePatientInfo(PatientInfo patient, SqlConnection connection, SqlTransaction transaction)
        {
            int patientInfoId = 0;
            using (SqlCommand cmd = new SqlCommand("usp_InsertPatientInfo", connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MobileNo", patient.MobileNo);
                cmd.Parameters.AddWithValue("@PatName", patient.PatName);
                cmd.Parameters.AddWithValue("@Age", patient.Age);
                cmd.Parameters.AddWithValue("@AgeType", patient.AgeType ?? (object)DBNull.Value); // Handle nullable
                cmd.Parameters.AddWithValue("@Gender", patient.Gender ?? (object)DBNull.Value); // Handle nullable
                cmd.Parameters.AddWithValue("@Ref", patient.Ref ?? (object)DBNull.Value); // Handle nullable
                cmd.Parameters.AddWithValue("@Area", patient.Area ?? (object)DBNull.Value); // Handle nullable
                cmd.Parameters.AddWithValue("@City", patient.City ?? (object)DBNull.Value); // Handle nullable
                cmd.Parameters.AddWithValue("@Email", patient.Email ?? (object)DBNull.Value); // Handle nullable

                SqlParameter outputIdParam = new SqlParameter("@PatientInfoId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);

                cmd.ExecuteNonQuery();
                patientInfoId = (int)outputIdParam.Value;
            }
            return patientInfoId;
        }

        private int SaveBillSummary(BillSummary summary, int patientInfoId, SqlConnection connection, SqlTransaction transaction)
        {
            int billSummaryId = 0;
            using (SqlCommand cmd = new SqlCommand("usp_InsertBillSummary", connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientInfoId", patientInfoId);
                cmd.Parameters.AddWithValue("@TotalBill", summary.TotalBill);
                cmd.Parameters.AddWithValue("@TotalDiscountAmount", summary.TotalDiscountAmount);
                cmd.Parameters.AddWithValue("@NetAmount", summary.NetAmount);
                cmd.Parameters.AddWithValue("@PaidAmount", summary.PaidAmount);
                cmd.Parameters.AddWithValue("@DueAmount", summary.DueAmount);
                cmd.Parameters.AddWithValue("@Remarks", summary.Remarks ?? (object)DBNull.Value); // Handle nullable

                SqlParameter outputIdParam = new SqlParameter("@BillSummaryId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);

                cmd.ExecuteNonQuery();
                billSummaryId = (int)outputIdParam.Value;
            }
            return billSummaryId;
        }

        private void SaveBillDetail(BillDetail detail, int billSummaryId, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand("usp_InsertBillDetail", connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                cmd.Parameters.AddWithValue("@InvId", detail.InvId);
                cmd.Parameters.AddWithValue("@InvName", detail.InvName);
                cmd.Parameters.AddWithValue("@Rate", detail.Rate);
                cmd.Parameters.AddWithValue("@DiscountAmount", detail.DiscountAmount);
                cmd.Parameters.AddWithValue("@DiscountPercent", detail.DiscountPercent);
                cmd.Parameters.AddWithValue("@NetAmount", detail.NetAmount);
                cmd.ExecuteNonQuery();
            }
        }

        private void SavePaymentDetail(PaymentDetail payment, int billSummaryId, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand("usp_InsertPaymentDetail", connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                cmd.Parameters.AddWithValue("@PaymentMode", payment.PaymentMode);
                cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                cmd.Parameters.AddWithValue("@RefNo", payment.RefNo ?? (object)DBNull.Value); // Handle nullable
                cmd.ExecuteNonQuery();
            }
        }

        // --- Main Orchestration Method ---

        // In DAL/PatientBilling/PatientBillingDB.cs
        public int SaveCompleteBill(PatientBillViewModel billData)
        {
            int billSummaryId = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    int patientInfoId;

                    // 1. Use existing PatientInfoId if provided, otherwise insert new
                    if (billData.PatientInfoId.HasValue && billData.PatientInfoId.Value > 0)
                    {
                        patientInfoId = billData.PatientInfoId.Value;
                    }
                    else
                    {
                        patientInfoId = SavePatientInfo(billData.PatientDetails, connection, transaction);
                    }

                    // 2. Save Bill Summary
                    billData.SummaryDetails.PatientInfoId = patientInfoId;
                    billSummaryId = SaveBillSummary(billData.SummaryDetails, patientInfoId, connection, transaction);

                    // 3. Save Bill Details
                    foreach (var detail in billData.BillDetails)
                    {
                        SaveBillDetail(detail, billSummaryId, connection, transaction);
                    }

                    // 4. Save Payment Details
                    foreach (var payment in billData.PaymentDetails)
                    {
                        SavePaymentDetail(payment, billSummaryId, connection, transaction);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return billSummaryId;
        }

        public CompleteBillData GetCompleteBillForPrint(int billSummaryId)
        {
            var result = new CompleteBillData();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    // Get Bill Summary
                    using (SqlCommand cmd = new SqlCommand("usp_GetBillSummary", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result.BillSummary = new BillSummary
                                {
                                    BillSummaryId = (int)reader["BillSummaryId"],
                                    PatientInfoId = (int)reader["PatientInfoId"],
                                    TotalBill = Convert.ToDecimal(reader["TotalBill"]),
                                    TotalDiscountAmount = Convert.ToDecimal(reader["TotalDiscountAmount"]),
                                    NetAmount = Convert.ToDecimal(reader["NetAmount"]),
                                    PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                                    DueAmount = Convert.ToDecimal(reader["DueAmount"]),
                                    Remarks = reader["Remarks"]?.ToString()
                                };
                            }
                        }
                    }

                    if (result.BillSummary != null)
                    {
                        // Get Patient Info
                        using (SqlCommand cmd = new SqlCommand("usp_GetPatientInfo", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PatientInfoId", result.BillSummary.PatientInfoId);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    result.PatientInfo = new PatientInfo
                                    {
                                        PatientInfoId = (int)reader["PatientInfoId"],
                                        MobileNo = reader["MobileNo"].ToString(),
                                        PatName = reader["PatName"].ToString(),
                                        Age = Convert.ToInt32(reader["Age"]),
                                        AgeType = reader["AgeType"]?.ToString(),
                                        Gender = reader["Gender"]?.ToString(),
                                        Ref = reader["Ref"]?.ToString(),
                                        Area = reader["Area"]?.ToString(),
                                        City = reader["City"]?.ToString(),
                                        Email = reader["Email"]?.ToString()
                                    };
                                }
                            }
                        }

                        // Get Bill Details
                        using (SqlCommand cmd = new SqlCommand("usp_GetBillDetails", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                            result.BillDetails = new List<BillDetail>();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    result.BillDetails.Add(new BillDetail
                                    {
                                        InvId = reader["InvId"].ToString(),
                                        InvName = reader["InvName"].ToString(),
                                        Rate = Convert.ToDecimal(reader["Rate"]),
                                        DiscountAmount = Convert.ToDecimal(reader["DiscountAmount"]),
                                        DiscountPercent = Convert.ToDecimal(reader["DiscountPercent"]),
                                        NetAmount = Convert.ToDecimal(reader["NetAmount"])
                                    });
                                }
                            }
                        }

                        // Get Payment Details
                        using (SqlCommand cmd = new SqlCommand("usp_GetPaymentDetails", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                            result.PaymentDetails = new List<PaymentDetail>();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    result.PaymentDetails.Add(new PaymentDetail
                                    {
                                        PaymentMode = reader["PaymentMode"].ToString(),
                                        Amount = Convert.ToDecimal(reader["Amount"]),
                                        RefNo = reader["RefNo"]?.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving bill data: " + ex.Message);
                    throw;
                }
            }

            return result;
        }
        public List<PatientInfo> SearchPatientsByMobile(string mobileNo)
        {
            var patients = new List<PatientInfo>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_SearchPatientsByMobile", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MobileNo", mobileNo);

                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patients.Add(new PatientInfo
                            {
                                PatientInfoId = (int)reader["PatientInfoId"],
                                UHID = reader["UHID"]?.ToString(),
                                PatName = reader["PatName"].ToString(),
                                MobileNo = reader["MobileNo"].ToString(),
                                Age = Convert.ToInt32(reader["Age"]),
                                AgeType = reader["AgeType"]?.ToString(),
                                Gender = reader["Gender"]?.ToString(),
                                Ref = reader["Ref"]?.ToString(),
                                Area = reader["Area"]?.ToString(),
                                City = reader["City"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                LastVisit = reader["LastVisit"] as DateTime?
                            });
                        }
                    }
                }
            }

            return patients;
        }
    }
}
