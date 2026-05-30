using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.Models
{
    public class LabMasterDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);

        public LabMaster Get_Current()
        {
            var result = new LabMaster
            {
                Id = 1,
                LabName = "SSK Diagnostics",
                ShowLogoInReport = true,
                ShowGSTInReport = true,
                ShowAccreditationInReport = true,
                Active = true
            };

            SqlCommand com = new SqlCommand("Usp_LabMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "Get");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                if (rdr.Read())
                {
                    result.Id = Convert.ToInt32(rdr["Id"]);
                    result.LabName = Convert.ToString(rdr["LabName"]);
                    result.ShortName = Convert.ToString(rdr["ShortName"]);
                    result.Tagline = Convert.ToString(rdr["Tagline"]);
                    result.AddressLine1 = Convert.ToString(rdr["AddressLine1"]);
                    result.AddressLine2 = Convert.ToString(rdr["AddressLine2"]);
                    result.City = Convert.ToString(rdr["City"]);
                    result.State = Convert.ToString(rdr["State"]);
                    result.Pincode = Convert.ToString(rdr["Pincode"]);
                    result.Country = Convert.ToString(rdr["Country"]);
                    result.MobileNumber = Convert.ToString(rdr["MobileNumber"]);
                    result.AlternateMobile = Convert.ToString(rdr["AlternateMobile"]);
                    result.Landline = Convert.ToString(rdr["Landline"]);
                    result.Email = Convert.ToString(rdr["Email"]);
                    result.Website = Convert.ToString(rdr["Website"]);
                    result.GSTNumber = Convert.ToString(rdr["GSTNumber"]);
                    result.PANNumber = Convert.ToString(rdr["PANNumber"]);
                    result.LabRegistrationNumber = Convert.ToString(rdr["LabRegistrationNumber"]);
                    result.NABLNumber = Convert.ToString(rdr["NABLNumber"]);
                    result.DrugLicenseNumber = Convert.ToString(rdr["DrugLicenseNumber"]);
                    result.ShowLogoInReport = Convert.ToBoolean(rdr["ShowLogoInReport"]);
                    result.ShowGSTInReport = Convert.ToBoolean(rdr["ShowGSTInReport"]);
                    result.ShowAccreditationInReport = Convert.ToBoolean(rdr["ShowAccreditationInReport"]);
                    result.ReceiptFooter = Convert.ToString(rdr["ReceiptFooter"]);
                    result.BranchName = Convert.ToString(rdr["BranchName"]);
                    result.BranchCode = Convert.ToString(rdr["BranchCode"]);
                    result.Active = Convert.ToBoolean(rdr["Active"]);
                    result.HasLogo = Convert.ToBoolean(rdr["HasLogo"]);
                    result.HasReportHeaderImage = Convert.ToBoolean(rdr["HasReportHeaderImage"]);
                    result.HasReportFooterImage = Convert.ToBoolean(rdr["HasReportFooterImage"]);
                }
            }
            catch
            {
            }
            finally
            {
                con.Close();
            }

            return result;
        }

        public Tuple<int, string> Save(LabMaster lab, byte[] logo, string logoMimeType, byte[] header, string headerMimeType, byte[] footer, string footerMimeType, string userName)
        {
            int statusCode = 0;
            string statusMsg = "";
            SqlCommand com = new SqlCommand("Usp_LabMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@LabName", (object)lab.LabName ?? DBNull.Value);
            com.Parameters.AddWithValue("@ShortName", (object)lab.ShortName ?? DBNull.Value);
            com.Parameters.AddWithValue("@Tagline", (object)lab.Tagline ?? DBNull.Value);
            com.Parameters.AddWithValue("@AddressLine1", (object)lab.AddressLine1 ?? DBNull.Value);
            com.Parameters.AddWithValue("@AddressLine2", (object)lab.AddressLine2 ?? DBNull.Value);
            com.Parameters.AddWithValue("@City", (object)lab.City ?? DBNull.Value);
            com.Parameters.AddWithValue("@State", (object)lab.State ?? DBNull.Value);
            com.Parameters.AddWithValue("@Pincode", (object)lab.Pincode ?? DBNull.Value);
            com.Parameters.AddWithValue("@Country", (object)lab.Country ?? DBNull.Value);
            com.Parameters.AddWithValue("@MobileNumber", (object)lab.MobileNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@AlternateMobile", (object)lab.AlternateMobile ?? DBNull.Value);
            com.Parameters.AddWithValue("@Landline", (object)lab.Landline ?? DBNull.Value);
            com.Parameters.AddWithValue("@Email", (object)lab.Email ?? DBNull.Value);
            com.Parameters.AddWithValue("@Website", (object)lab.Website ?? DBNull.Value);
            com.Parameters.AddWithValue("@GSTNumber", (object)lab.GSTNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@PANNumber", (object)lab.PANNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@LabRegistrationNumber", (object)lab.LabRegistrationNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@NABLNumber", (object)lab.NABLNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@DrugLicenseNumber", (object)lab.DrugLicenseNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@ShowLogoInReport", lab.ShowLogoInReport);
            com.Parameters.AddWithValue("@ShowGSTInReport", lab.ShowGSTInReport);
            com.Parameters.AddWithValue("@ShowAccreditationInReport", lab.ShowAccreditationInReport);
            com.Parameters.AddWithValue("@ReceiptFooter", (object)lab.ReceiptFooter ?? DBNull.Value);
            com.Parameters.AddWithValue("@BranchName", (object)lab.BranchName ?? DBNull.Value);
            com.Parameters.AddWithValue("@BranchCode", (object)lab.BranchCode ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", lab.Active);
            com.Parameters.Add("@Logo", SqlDbType.VarBinary, -1).Value = (object)logo ?? DBNull.Value;
            com.Parameters.Add("@LogoMimeType", SqlDbType.VarChar, 50).Value = (object)logoMimeType ?? DBNull.Value;
            com.Parameters.Add("@ReportHeaderImage", SqlDbType.VarBinary, -1).Value = (object)header ?? DBNull.Value;
            com.Parameters.Add("@ReportHeaderMimeType", SqlDbType.VarChar, 50).Value = (object)headerMimeType ?? DBNull.Value;
            com.Parameters.Add("@ReportFooterImage", SqlDbType.VarBinary, -1).Value = (object)footer ?? DBNull.Value;
            com.Parameters.Add("@ReportFooterMimeType", SqlDbType.VarChar, 50).Value = (object)footerMimeType ?? DBNull.Value;
            com.Parameters.AddWithValue("@UpdatedBy", (object)userName ?? DBNull.Value);
            com.Parameters.AddWithValue("@Action", "Save");
            com.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                com.ExecuteNonQuery();
                statusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                statusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception ex)
            {
                statusCode = 0;
                statusMsg = ex.Message;
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(statusCode, statusMsg);
        }

        public Tuple<byte[], string> Get_Image(string imageType)
        {
            SqlCommand com = new SqlCommand("Usp_LabMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ImageType", (object)imageType ?? "Logo");
            com.Parameters.AddWithValue("@Action", "GetImage");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                if (rdr.Read())
                {
                    byte[] img = rdr["ImageData"] == DBNull.Value ? null : (byte[])rdr["ImageData"];
                    string mime = rdr["ImageMimeType"] == DBNull.Value ? null : Convert.ToString(rdr["ImageMimeType"]);
                    return new Tuple<byte[], string>(img, mime);
                }
            }
            catch
            {
            }
            finally
            {
                con.Close();
            }
            return new Tuple<byte[], string>(null, null);
        }
    }
}
