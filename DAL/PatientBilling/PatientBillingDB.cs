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

        public int SaveCompleteBill(PatientBillViewModel billData)
        {
            int billSummaryId = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Save Patient Info
                    int patientInfoId = SavePatientInfo(billData.PatientDetails, connection, transaction);

                    // 2. Save Bill Summary (using the newly obtained PatientInfoId)
                    billData.SummaryDetails.PatientInfoId = patientInfoId; // Ensure FK is set
                    billSummaryId = SaveBillSummary(billData.SummaryDetails, patientInfoId, connection, transaction);

                    // 3. Save Bill Details (Investigations)
                    foreach (var detail in billData.BillDetails)
                    {
                        SaveBillDetail(detail, billSummaryId, connection, transaction);
                    }

                    // 4. Save Payment Details
                    foreach (var payment in billData.PaymentDetails)
                    {
                        SavePaymentDetail(payment, billSummaryId, connection, transaction);
                    }

                    transaction.Commit(); // Commit the transaction if all saves are successful
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback on error
                    // Log the exception (e.g., using a logging framework)
                    Console.WriteLine("Error saving bill: " + ex.Message);
                    throw; // Re-throw to be handled by the controller
                }
            }
            return billSummaryId; // Return the generated BillSummaryId
        }
    }
}