using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ParameterHeaderDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        private int StatusCode;
        private string StatusMsg = "";

        public Tuple<int, string> Add_ParameterHeader(ParameterHeader header)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ParameterHeader", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", header.Id);
            com.Parameters.AddWithValue("@HeaderName", header.HeaderName);
            com.Parameters.AddWithValue("@Active", header.Active);
            com.Parameters.AddWithValue("@Action", "Add");
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
                // swallow like other DB classes
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<ParameterHeader> Get_ParameterHeader()
        {
            List<ParameterHeader> headers = new List<ParameterHeader>();
            SqlCommand com = new SqlCommand("Usp_ParameterHeader", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetHeader");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                headers.Add(new ParameterHeader
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    HeaderName = rdr["HeaderName"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])
                });
            }
            return headers;
        }

        public Tuple<int, string> Update_ParameterHeader(ParameterHeader header)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ParameterHeader", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", header.Id);
            com.Parameters.AddWithValue("@HeaderName", header.HeaderName);
            com.Parameters.AddWithValue("@Active", header.Active);
            com.Parameters.AddWithValue("@Action", "Update");
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
                // swallow like other DB classes
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<ParameterHeader> Get_ParameterHeaderById(int Id)
        {
            List<ParameterHeader> headers = new List<ParameterHeader>();
            SqlCommand com = new SqlCommand("Usp_ParameterHeader", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetHeaderById");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                headers.Add(new ParameterHeader
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    HeaderName = rdr["HeaderName"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])
                });
            }
            return headers;
        }

        public Tuple<int, string> Delete_ParameterHeader(int Id)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ParameterHeader", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "Delete");
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
                // swallow like other DB classes
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }
    }
}
