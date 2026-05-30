using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.Models
{
    public class PackageMasterDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);

        public List<PackageMaster> Get_Packages()
        {
            var list = new List<PackageMaster>();
            SqlCommand com = new SqlCommand("Usp_PackageMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetList");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new PackageMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        PackageCode = Convert.ToString(rdr["PackageCode"]),
                        PackageName = Convert.ToString(rdr["PackageName"]),
                        ReportingName = Convert.ToString(rdr["ReportingName"]),
                        Price = Convert.ToDecimal(rdr["Price"]),
                        DiscountAmount = Convert.ToDecimal(rdr["DiscountAmount"]),
                        Description = Convert.ToString(rdr["Description"]),
                        Active = Convert.ToBoolean(rdr["Active"]),
                        Investigations = Convert.ToString(rdr["Investigations"]),
                        InvestigationCount = Convert.ToInt32(rdr["InvestigationCount"])
                    });
                }
            }
            catch
            {
            }
            finally
            {
                con.Close();
            }
            return list;
        }

        public List<PackageMaster> Get_ActivePackages()
        {
            var list = new List<PackageMaster>();
            SqlCommand com = new SqlCommand("Usp_PackageMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetActive");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new PackageMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        PackageCode = Convert.ToString(rdr["PackageCode"]),
                        PackageName = Convert.ToString(rdr["PackageName"]),
                        ReportingName = Convert.ToString(rdr["ReportingName"]),
                        Price = Convert.ToDecimal(rdr["Price"]),
                        DiscountAmount = Convert.ToDecimal(rdr["DiscountAmount"]),
                        Active = Convert.ToBoolean(rdr["Active"])
                    });
                }
            }
            catch
            {
            }
            finally
            {
                con.Close();
            }
            return list;
        }

        public List<PackageMaster> Get_PackageById(int id)
        {
            var list = new List<PackageMaster>();
            SqlCommand com = new SqlCommand("Usp_PackageMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", id);
            com.Parameters.AddWithValue("@Action", "GetById");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new PackageMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        PackageCode = Convert.ToString(rdr["PackageCode"]),
                        PackageName = Convert.ToString(rdr["PackageName"]),
                        ReportingName = Convert.ToString(rdr["ReportingName"]),
                        Price = Convert.ToDecimal(rdr["Price"]),
                        DiscountAmount = Convert.ToDecimal(rdr["DiscountAmount"]),
                        Description = Convert.ToString(rdr["Description"]),
                        Active = Convert.ToBoolean(rdr["Active"]),
                        InvestigationIds = Convert.ToString(rdr["InvestigationIds"])
                    });
                }
            }
            catch
            {
            }
            finally
            {
                con.Close();
            }
            return list;
        }

        public Tuple<int, string> Add_Package(PackageMaster model)
        {
            return Save_Package(model, "Add");
        }

        public Tuple<int, string> Update_Package(PackageMaster model)
        {
            return Save_Package(model, "Update");
        }

        public Tuple<int, string> Set_PackageActive(int id, bool active)
        {
            int statusCode = 0;
            string statusMsg = "";
            SqlCommand com = new SqlCommand("Usp_PackageMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", id);
            com.Parameters.AddWithValue("@Active", active);
            com.Parameters.AddWithValue("@Action", "SetActive");
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

        public List<PackageInvestigationItem> Get_PackageInvestigations(int packageId)
        {
            var list = new List<PackageInvestigationItem>();
            SqlCommand com = new SqlCommand("Usp_PackageMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", packageId);
            com.Parameters.AddWithValue("@Action", "GetInvestigations");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new PackageInvestigationItem
                    {
                        Id = Convert.ToString(rdr["Id"]),
                        InvCode = Convert.ToString(rdr["InvCode"]),
                        InvName = Convert.ToString(rdr["InvName"]),
                        Rate = Convert.ToDecimal(rdr["Rate"])
                    });
                }
            }
            catch
            {
            }
            finally
            {
                con.Close();
            }
            return list;
        }

        private Tuple<int, string> Save_Package(PackageMaster model, string action)
        {
            int statusCode = 0;
            string statusMsg = "";
            SqlCommand com = new SqlCommand("Usp_PackageMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", model.Id);
            com.Parameters.AddWithValue("@PackageCode", (object)model.PackageCode ?? DBNull.Value);
            com.Parameters.AddWithValue("@PackageName", (object)model.PackageName ?? DBNull.Value);
            com.Parameters.AddWithValue("@ReportingName", (object)model.ReportingName ?? DBNull.Value);
            com.Parameters.AddWithValue("@Price", model.Price);
            com.Parameters.AddWithValue("@DiscountAmount", model.DiscountAmount);
            com.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", model.Active);
            com.Parameters.AddWithValue("@InvestigationIds", (object)model.InvestigationIds ?? DBNull.Value);
            com.Parameters.AddWithValue("@Action", action);
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
    }
}
