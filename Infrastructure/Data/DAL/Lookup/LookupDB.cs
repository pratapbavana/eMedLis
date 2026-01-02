using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using eMedLis.Domain.Configuration.Entities;

namespace eMedLis.Infrastructure.Data.Lookup
{
    public class LookupDB
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        int StatusCode;
        string StatusMsg = "";
        public Tuple<int, string> Add_Record(Lookups dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_Lookup", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@ItemName", dp.ItemName);
            com.Parameters.AddWithValue("@ItemType", dp.ItemType);
            com.Parameters.AddWithValue("@Active", dp.Active);
            com.Parameters.AddWithValue("@Action", "Add");
            //com.Parameters.AddWithValue("@CreateBy", cm);
            //com.Parameters.AddWithValue("@Terminal", cm);
            //com.Parameters.AddWithValue("@CreateDate", cm);
            com.Parameters.Add("@StatusCode", SqlDbType.Int);
            com.Parameters["@StatusCode"].Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100);
            com.Parameters["@StatusMsg"].Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                i = com.ExecuteNonQuery();
                StatusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                StatusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception ex)
            {
                // throw the exception  
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }
        //Get List Of Countries
        public List<Lookups> Get_Record(string ItemType)
        {
            List<Lookups> rec = new List<Lookups>();
            SqlCommand com = new SqlCommand("Usp_Lookup", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ItemType",ItemType);
            com.Parameters.AddWithValue("@Action", "GetRecords");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                rec.Add(new Lookups
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    ItemName = rdr["ItemName"].ToString(),
                    ItemType = rdr["ItemType"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return rec;

        }

        //Update Lookup
        public Tuple<int, string> Update_Record(Lookups dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_Lookup", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@ItemName", dp.ItemName);
            com.Parameters.AddWithValue("@ItemType", dp.ItemType);
            com.Parameters.AddWithValue("@Active", dp.Active);
            com.Parameters.AddWithValue("@Action", "Update");
            //com.Parameters.AddWithValue("@CreateBy", cm);
            //com.Parameters.AddWithValue("@Terminal", cm);
            //com.Parameters.AddWithValue("@CreateDate", cm);
            com.Parameters.Add("@StatusCode", SqlDbType.Int);
            com.Parameters["@StatusCode"].Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100);
            com.Parameters["@StatusMsg"].Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                i = com.ExecuteNonQuery();
                StatusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                StatusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception ex)
            {
                // throw the exception  
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        //Get Record By Id
        public List<Lookups> Get_RecordsById(int Id)
        {
            List<Lookups> rec = new List<Lookups>();
            SqlCommand com = new SqlCommand("Usp_Lookup", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetRecordsById");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                rec.Add(new Lookups
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    ItemName = rdr["ItemName"].ToString(),
                    ItemType = rdr["ItemType"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return rec;
        }

        public int Delete_Record(int Id)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_Lookup", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "Delete");
            con.Open();
            i = com.ExecuteNonQuery();
            con.Close();
            return i;
        }

    }
}