using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class SubDepartmentDB
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        int StatusCode;
        string StatusMsg = "";
        public Tuple<int , string> Add_SubDepartment(SubDepartment dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_SubDepartment", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@SubDeptName", dp.SubDeptName);
            com.Parameters.AddWithValue("@DeptId", dp.DepartmentId);
            com.Parameters.AddWithValue("@Header", dp.Header);
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
        public List<SubDepartment> Get_SubDepartment()
        {
            List<SubDepartment> dept = new List<SubDepartment>();
            SqlCommand com = new SqlCommand("Usp_SubDepartment", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetSubDepartment");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                dept.Add(new SubDepartment
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    SubDeptName = rdr["SubDeptName"].ToString(),
                    DepartmentName = rdr["DepartmentName"].ToString(),
                   
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return dept;

        }

        //Update Country
        
        public Tuple<int, string> Update_SubDepartment(SubDepartment dp)
        {
            int i;
            
            SqlCommand com = new SqlCommand("Usp_SubDepartment", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@SubDeptName", dp.SubDeptName);
            com.Parameters.AddWithValue("@DeptId", dp.DepartmentId);
            com.Parameters.AddWithValue("@Header", dp.Header);
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
               
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        //Get Record By Id
        public List<SubDepartment> Get_SubDepartmentById(int Id)
        {
            List<SubDepartment> subdept = new List<SubDepartment>();
            SqlCommand com = new SqlCommand("Usp_SubDepartment", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetSubDepartmentById");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                subdept.Add(new SubDepartment
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    SubDeptName = rdr["SubDeptName"].ToString(),
                    DepartmentId = Convert.ToInt32(rdr["DepartmentId"]),
                    DepartmentName = rdr["DepartmentName"].ToString(),
                    Header = rdr["Header"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return subdept;
        }

        public List<SubDepartment> Get_SubDepartmentByDeptId(int DeptId)
        {
            List<SubDepartment> dept = new List<SubDepartment>();
            SqlCommand com = new SqlCommand("Usp_SubDepartment", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@DeptId", DeptId);
            com.Parameters.AddWithValue("@Action", "GetSubDepartmentByDeptId");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                dept.Add(new SubDepartment
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    SubDeptName = rdr["SubDeptName"].ToString(),
                    DepartmentId = Convert.ToInt32(rdr["DepartmentId"]),
                    DepartmentName = rdr["DepartmentName"].ToString(),
                    Header = rdr["Header"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return dept;
        }
    }
}