using eMedLis.Models.ReportSettings;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.DAL.ReportSettings
{
    public class ReportSettingsDB
    {
        private readonly string _connectionString;

        public ReportSettingsDB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;
        }

        public ReportLayoutSettings GetCurrent()
        {
            var result = ReportLayoutSettings.CreateDefault();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportSettings", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "Get");
                cmd.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        result.PrintMode = Convert.ToString(rdr["PrintMode"]);
                        result.PrintHeader = Convert.ToBoolean(rdr["PrintHeader"]);
                        result.HeaderHeightPx = Convert.ToInt32(rdr["HeaderHeightPx"]);
                        result.ShowLogo = Convert.ToBoolean(rdr["ShowLogo"]);
                        result.ShowLabDetails = Convert.ToBoolean(rdr["ShowLabDetails"]);
                        result.PrintFooter = Convert.ToBoolean(rdr["PrintFooter"]);
                        result.FooterHeightPx = Convert.ToInt32(rdr["FooterHeightPx"]);
                        result.FooterText = Convert.ToString(rdr["FooterText"]);
                        result.TopMarginPx = Convert.ToInt32(rdr["TopMarginPx"]);
                        result.LeftMarginPx = Convert.ToInt32(rdr["LeftMarginPx"]);
                        result.RightMarginPx = Convert.ToInt32(rdr["RightMarginPx"]);
                        result.BottomMarginPx = Convert.ToInt32(rdr["BottomMarginPx"]);
                        result.ContentStartPx = Convert.ToInt32(rdr["ContentStartPx"]);
                        result.LabName = Convert.ToString(rdr["LabName"]);
                        result.LabAddress = Convert.ToString(rdr["LabAddress"]);
                        result.LabPhone = Convert.ToString(rdr["LabPhone"]);
                    }
                }
            }

            return result;
        }

        public Tuple<int, string> Save(ReportLayoutSettings settings, string userName)
        {
            if (settings == null)
            {
                return new Tuple<int, string>(0, "Invalid settings");
            }

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Usp_ReportSettings", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PrintMode", (object)(settings.PrintMode ?? "PlainPaper"));
                cmd.Parameters.AddWithValue("@PrintHeader", settings.PrintHeader);
                cmd.Parameters.AddWithValue("@HeaderHeightPx", settings.HeaderHeightPx);
                cmd.Parameters.AddWithValue("@ShowLogo", settings.ShowLogo);
                cmd.Parameters.AddWithValue("@ShowLabDetails", settings.ShowLabDetails);
                cmd.Parameters.AddWithValue("@PrintFooter", settings.PrintFooter);
                cmd.Parameters.AddWithValue("@FooterHeightPx", settings.FooterHeightPx);
                cmd.Parameters.AddWithValue("@FooterText", (object)(settings.FooterText ?? string.Empty));
                cmd.Parameters.AddWithValue("@TopMarginPx", settings.TopMarginPx);
                cmd.Parameters.AddWithValue("@LeftMarginPx", settings.LeftMarginPx);
                cmd.Parameters.AddWithValue("@RightMarginPx", settings.RightMarginPx);
                cmd.Parameters.AddWithValue("@BottomMarginPx", settings.BottomMarginPx);
                cmd.Parameters.AddWithValue("@ContentStartPx", settings.ContentStartPx);
                cmd.Parameters.AddWithValue("@LabName", (object)(settings.LabName ?? string.Empty));
                cmd.Parameters.AddWithValue("@LabAddress", (object)(settings.LabAddress ?? string.Empty));
                cmd.Parameters.AddWithValue("@LabPhone", (object)(settings.LabPhone ?? string.Empty));
                cmd.Parameters.AddWithValue("@UpdatedBy", (object)(userName ?? string.Empty));
                cmd.Parameters.AddWithValue("@Action", "Save");
                cmd.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();

                var code = Convert.ToInt32(cmd.Parameters["@StatusCode"].Value);
                var msg = Convert.ToString(cmd.Parameters["@StatusMsg"].Value);
                return new Tuple<int, string>(code, msg);
            }
        }
    }
}
