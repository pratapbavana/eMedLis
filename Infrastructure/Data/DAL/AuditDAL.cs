using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace eMedLis.Infrastructure.Data
{
    public class AuditDAL
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;

        /// <summary>
        /// Log user action to audit table
        /// </summary>
        public static void LogAction(int userId, string action, string controller, string method,
                                    string ipAddress, string userAgent, string details = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO AuditLogs (UserId, Action, Controller, Method, IpAddress, UserAgent, Details, Timestamp)
                        VALUES (@UserId, @Action, @Controller, @Method, @IpAddress, @UserAgent, @Details, GETUTCDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@Controller", controller ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Method", method ?? string.Empty);
                        cmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? string.Empty);
                        cmd.Parameters.AddWithValue("@UserAgent", userAgent ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Details", details ?? string.Empty);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - audit failure shouldn't break application
                System.Diagnostics.EventLog.WriteEntry("eMedLis", $"Audit logging failed: {ex.Message}");
            }
        }
    }
}
