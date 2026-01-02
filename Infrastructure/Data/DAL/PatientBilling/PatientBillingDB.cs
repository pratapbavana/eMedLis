using System;
using System.Collections.Generic;
using System.Configuration; // For ConfigurationManager
using System.Data;
using System.Data.SqlClient;
using eMedLis.Domain.PatientBilling.Entities;
using eMedLis.Domain.PatientBilling.ViewModels;

namespace eMedLis.Infrastructure.Data.PatientBilling
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
            string generatedBillNo = "";

            using (SqlCommand cmd = new SqlCommand("usp_InsertBillSummary", connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientInfoId", patientInfoId);
                cmd.Parameters.AddWithValue("@TotalBill", summary.TotalBill);
                cmd.Parameters.AddWithValue("@TotalDiscountAmount", summary.TotalDiscountAmount);
                cmd.Parameters.AddWithValue("@NetAmount", summary.NetAmount);
                cmd.Parameters.AddWithValue("@PaidAmount", summary.PaidAmount);
                cmd.Parameters.AddWithValue("@DueAmount", summary.DueAmount);
                cmd.Parameters.AddWithValue("@Remarks", summary.Remarks ?? (object)DBNull.Value);

                SqlParameter outputIdParam = new SqlParameter("@BillSummaryId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);

                // Execute and get multiple result sets
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // First result set contains the generated BillNo
                    if (reader.Read())
                    {
                        generatedBillNo = reader["GeneratedBillNo"].ToString();
                    }
                }

                billSummaryId = (int)outputIdParam.Value;

                // Store the generated BillNo in the summary object for later use
                summary.BillNo = generatedBillNo;
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
        private void SaveInitialPayments(int billSummaryId,
                                 List<PaymentDetail> payments,
                                 SqlConnection conn,
                                 SqlTransaction tx)
        {
            // 1. Generate receipt number for initial payment
            string receiptNo;
            using (var cmd = new SqlCommand("usp_GenerateReceiptNo", conn, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Prefix", "RCP");
                var outParam = new SqlParameter("@ReceiptNo", SqlDbType.VarChar, 20)
                { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outParam);
                cmd.ExecuteNonQuery();
                receiptNo = outParam.Value.ToString();
            }

            // 2. Insert each PaymentDetail with same receipt
            foreach (var p in payments)
            {
                using (var cmd = new SqlCommand("usp_InsertPaymentDetail", conn, tx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                    cmd.Parameters.AddWithValue("@PaymentMode", p.PaymentMode);
                    cmd.Parameters.AddWithValue("@Amount", p.Amount);
                    cmd.Parameters.AddWithValue("@RefNo", (object)p.RefNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDuePayment", 0);
                    cmd.Parameters.AddWithValue("@ReceiptNo", receiptNo);
                    cmd.ExecuteNonQuery();
                }
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
                cmd.Parameters.AddWithValue("@IsDuePayment", payment.IsDuePayment);
                cmd.ExecuteNonQuery();
                cmd.ExecuteNonQuery();
            }
        }

        // --- Main Orchestration Method ---

        // In DAL/PatientBilling/PatientBillingDB.cs
        public BillSaveResult SaveCompleteBill(PatientBillViewModel billData)
        {
            int billSummaryId = 0;
            string billNo = "";

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

                    // 2. Save Bill Summary (this will generate BillNo)
                    billData.SummaryDetails.PatientInfoId = patientInfoId;
                    billSummaryId = SaveBillSummary(billData.SummaryDetails, patientInfoId, connection, transaction);
                    billNo = billData.SummaryDetails.BillNo; // Get the generated BillNo

                    // 3. Save Bill Details
                    foreach (var detail in billData.BillDetails)
                    {
                        SaveBillDetail(detail, billSummaryId, connection, transaction);
                    }

                    // 4. Save Payment Details
                    SaveInitialPayments(billSummaryId, billData.PaymentDetails, connection, transaction);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return new BillSaveResult
            {
                BillSummaryId = billSummaryId,
                BillNo = billNo,
                Success = true
            };
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
                                    BillNo = reader["BillNo"]?.ToString(),
                                    PatientInfoId = (int)reader["PatientInfoId"],
                                    TotalBill = Convert.ToDecimal(reader["TotalBill"]),
                                    TotalDiscountAmount = Convert.ToDecimal(reader["TotalDiscountAmount"]),
                                    NetAmount = Convert.ToDecimal(reader["NetAmount"]),
                                    PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                                    DueAmount = Convert.ToDecimal(reader["DueAmount"]),
                                    Remarks = reader["Remarks"]?.ToString(),
                                    BillDate = Convert.ToDateTime(reader["BillDate"])
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
                                        UHID = reader["UHID"]?.ToString(),
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
                                        RefNo = reader["RefNo"]?.ToString(),
                                        ReceiptNo = reader["ReceiptNo"]?.ToString(),
                                        PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
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
            return SearchPatientsUniversal(mobileNo);
        }
        public List<PatientInfo> SearchPatientsUniversal(string searchValue)
        {
            var patients = new List<PatientInfo>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_SearchPatientsUniversal", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue);

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
        public CompleteBillData GetBillByBillNo(string billNo)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_GetBillByBillNo", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", billNo);

                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int billSummaryId = (int)reader["BillSummaryId"];
                            return GetCompleteBillForPrint(billSummaryId);
                        }
                    }
                }
            }
            return null;
        }
        public List<BillListItem> GetRecentBills(int days = 30, string status = "")
        {
            var bills = new List<BillListItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_GetRecentBills", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Days", days);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status)
                        ? (object)DBNull.Value
                        : status);

                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // First result set - Bills
                        while (reader.Read())
                        {
                            bills.Add(new BillListItem
                            {
                                BillSummaryId = (int)reader["BillSummaryId"],
                                BillNo = reader["BillNo"]?.ToString(),
                                BillDate = Convert.ToDateTime(reader["BillDate"]),
                                TotalBill = Convert.ToDecimal(reader["TotalBill"]),
                                PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                                DueAmount = Convert.ToDecimal(reader["DueAmount"]),
                                NetAmount = Convert.ToDecimal(reader["NetAmount"]),
                                PatName = reader["PatName"].ToString(),
                                Age = Convert.ToInt32(reader["Age"]),
                                AgeType = reader["AgeType"]?.ToString(),
                                Gender = reader["Gender"]?.ToString(),
                                Ref = reader["Ref"]?.ToString() ?? "Self",
                                MobileNo = reader["MobileNo"].ToString(),
                                UHID = reader["UHID"]?.ToString(),
                                PaymentStatus = reader["PaymentStatus"].ToString()
                            });
                        }
                    }
                }
            }

            return bills;
        }
        public bool CancelBill(int billSummaryId, string cancelReason, string cancelledBy)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_CancelBill", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                    cmd.Parameters.AddWithValue("@CancelReason", cancelReason);
                    cmd.Parameters.AddWithValue("@CancelledBy", cancelledBy);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public List<DueBillItem> GetDueBills(int days = 30)
        {
            var bills = new List<DueBillItem>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("usp_GetDueBills", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Days", days);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bills.Add(new DueBillItem
                        {
                            BillSummaryId = (int)reader["BillSummaryId"],
                            BillNo = reader["BillNo"].ToString(),
                            BillDate = Convert.ToDateTime(reader["BillDate"]),
                            NetAmount = Convert.ToDecimal(reader["NetAmount"]),
                            PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                            DueAmount = Convert.ToDecimal(reader["DueAmount"]),
                            PatName = reader["PatName"].ToString(),
                            MobileNo = reader["MobileNo"].ToString(),
                            UHID = reader["UHID"]?.ToString(),
                            Age = Convert.ToInt32(reader["Age"]),
                            Gender = reader["Gender"]?.ToString(),
                            DaysPending = Convert.ToInt32(reader["DaysPending"])
                        });
                    }
                }
            }

            return bills;
        }
        public class DuePaymentResult
        {
            public int DuePaymentId { get; set; }
            public string ReceiptNo { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public DuePaymentResult ProcessDuePayment(DuePayment payment)
        {
            var result = new DuePaymentResult();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("usp_InsertDuePayment", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BillSummaryId", payment.BillSummaryId);
                cmd.Parameters.AddWithValue("@PaymentMode", payment.PaymentMode);
                cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                cmd.Parameters.AddWithValue("@RefNo", (object)payment.RefNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ReceivedBy", payment.ReceivedBy ?? "System");
                cmd.Parameters.AddWithValue("@Remarks", (object)payment.Remarks ?? DBNull.Value);

                var idParam = new SqlParameter("@DuePaymentId", SqlDbType.Int)
                { Direction = ParameterDirection.Output };
                var rcptParam = new SqlParameter("@ReceiptNo", SqlDbType.VarChar, 20)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(idParam);
                cmd.Parameters.Add(rcptParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                result.DuePaymentId = (int)idParam.Value;
                result.ReceiptNo = rcptParam.Value.ToString();
                result.Success = true;
            }
            return result;
        }
        public PaymentReceiptData GetPaymentReceipt(int duePaymentId)
        {
            PaymentReceiptData receiptData = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("usp_GetPaymentReceipt", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DuePaymentId", duePaymentId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        receiptData = new PaymentReceiptData
                        {
                            Payment = new DuePayment
                            {
                                DuePaymentId = (int)reader["DuePaymentId"],
                                ReceiptNo = reader["ReceiptNo"].ToString(),
                                PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                                PaymentMode = reader["PaymentMode"].ToString(),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                RefNo = reader["RefNo"]?.ToString(),
                                ReceivedBy = reader["ReceivedBy"]?.ToString(),
                                Remarks = reader["Remarks"]?.ToString()
                            },
                            Bill = new BillSummary
                            {
                                BillSummaryId = (int)reader["BillSummaryId"],
                                BillNo = reader["BillNo"].ToString(),
                                NetAmount = Convert.ToDecimal(reader["BillAmount"]),
                                PaidAmount = Convert.ToDecimal(reader["TotalPaid"]),
                                DueAmount = Convert.ToDecimal(reader["RemainingDue"])
                            },
                            Patient = new PatientInfo
                            {
                                PatName = reader["PatName"].ToString(),
                                MobileNo = reader["MobileNo"].ToString(),
                                UHID = reader["UHID"]?.ToString(),
                                Age = Convert.ToInt32(reader["Age"]),
                                Gender = reader["Gender"]?.ToString()
                            }
                        };
                    }
                }
            }

            return receiptData;
        }
    }
}
