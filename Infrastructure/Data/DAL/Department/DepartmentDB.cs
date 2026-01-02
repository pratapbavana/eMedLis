using eMedLis.Domain.Configuration.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMedLis.Infrastructure.Data.Department
{
    public class DepartmentDB
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        int StatusCode;
        string StatusMsg = "";
        public Tuple<int, string> Add_Department(Departments dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_Department", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@DepartmentName", dp.DepartmentName);
            com.Parameters.AddWithValue("@Description", dp.Description);
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
        public List<Departments> Get_Department()
        {
            List<Departments> dept = new List<Departments>();
            SqlCommand com = new SqlCommand("Usp_Department", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetDepartment");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                dept.Add(new Departments
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    DepartmentName = rdr["DepartmentName"].ToString(),
                    Description = rdr["Description"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return dept;

        }

        //Update Department
        public Tuple<int, string> Update_Department(Departments dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_Department", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@DepartmentName", dp.DepartmentName);
            com.Parameters.AddWithValue("@Description", dp.Description);
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
        public List<Departments> Get_DepartmentById(int Id)
        {
            List<Departments> dept = new List<Departments>();
            SqlCommand com = new SqlCommand("Usp_Department", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetDepartmentById");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                dept.Add(new Departments
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    DepartmentName = rdr["DepartmentName"].ToString(),
                    Description = rdr["Description"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return dept;
        }

        public int Delete_Department(int DepartmentId)
        {
            int i;
            SqlCommand com = new SqlCommand("sp_DepartmentMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@DepartmentId", DepartmentId);
            com.Parameters.AddWithValue("@Action", "Delete");
            con.Open();
            i = com.ExecuteNonQuery();
            con.Close();
            return i;
        }


    }
}