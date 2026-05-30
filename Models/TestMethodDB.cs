using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class TestMethodDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        private int StatusCode;
        private string StatusMsg = "";

        public Tuple<int, string> Add_TestMethod(TestMethod method)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_TestMethod", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", method.Id);
            com.Parameters.AddWithValue("@MethodName", method.MethodName);
            com.Parameters.AddWithValue("@Active", method.Active);
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
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<TestMethod> Get_TestMethod()
        {
            List<TestMethod> methods = new List<TestMethod>();
            SqlCommand com = new SqlCommand("Usp_TestMethod", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetMethods");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    methods.Add(new TestMethod
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        MethodName = rdr["MethodName"].ToString(),
                        Active = Convert.ToBoolean(rdr["Active"])
                    });
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }
            return methods;
        }

        public Tuple<int, string> Update_TestMethod(TestMethod method)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_TestMethod", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", method.Id);
            com.Parameters.AddWithValue("@MethodName", method.MethodName);
            com.Parameters.AddWithValue("@Active", method.Active);
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
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<TestMethod> Get_TestMethodById(int Id)
        {
            List<TestMethod> methods = new List<TestMethod>();
            SqlCommand com = new SqlCommand("Usp_TestMethod", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetMethodById");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    methods.Add(new TestMethod
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        MethodName = rdr["MethodName"].ToString(),
                        Active = Convert.ToBoolean(rdr["Active"])
                    });
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }
            return methods;
        }

        public Tuple<int, string> Delete_TestMethod(int Id)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_TestMethod", con);
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
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }
    }
}
