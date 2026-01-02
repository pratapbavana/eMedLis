using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using eMedLis.Domain.Laboratory.Entities;

namespace eMedLis.Infrastructure.Data.InvMaster
{
    public class InvMasterDB
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        int StatusCode;
        string StatusMsg = "";
        public Tuple<int, string> Add_Investigation(InvMasters dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_InvMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@InvName", dp.InvName);
            com.Parameters.AddWithValue("@ReportHdr", dp.ReportHdr);
            com.Parameters.AddWithValue("@Rate", dp.Rate);
            com.Parameters.AddWithValue("@SubDeptId", dp.SubDeptId);
            com.Parameters.AddWithValue("@SpecimenId", dp.SpecimenId);
            com.Parameters.AddWithValue("@VacutainerId", dp.VacutainerId);
            com.Parameters.AddWithValue("@ReportTime", dp.ReportTime);
            com.Parameters.AddWithValue("@InvCode", dp.InvCode);
            com.Parameters.AddWithValue("@GuideLines", dp.GuideLines);
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
               
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }
        //Get List Of Countries
        public List<InvMasters> Get_Investigation()
        {
            List<InvMasters> inv = new List<InvMasters>();
            SqlCommand com = new SqlCommand("Usp_InvMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetInvestigation");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                inv.Add(new InvMasters
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    InvName = rdr["InvName"].ToString(),
                    SubDeptName = rdr["SubDeptName"].ToString(),
                    ReportHdr = rdr["ReportHdr"].ToString(),
                    Rate = Convert.ToDecimal(rdr["Rate"]),
                    SubDeptId = Convert.ToInt32(rdr["SubDeptId"]),
                    SpecimenId = Convert.ToInt32(rdr["SpecimenId"]),
                    VacutainerId = Convert.ToInt32(rdr["VacutainerId"]),
                    ReportTime = rdr["ReportTime"].ToString(),
                    InvCode = rdr["InvCode"].ToString(),
                    GuideLines = rdr["GuideLines"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return inv;

        }

        //Update Inv
        public Tuple<int, string> Update_Investigation(InvMasters dp)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_InvMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", dp.Id);
            com.Parameters.AddWithValue("@InvName", dp.InvName);
            com.Parameters.AddWithValue("@ReportHdr", dp.ReportHdr);
            com.Parameters.AddWithValue("@Rate", dp.Rate);
            com.Parameters.AddWithValue("@SubDeptId", dp.SubDeptId);
            com.Parameters.AddWithValue("@SpecimenId", dp.SpecimenId);
            com.Parameters.AddWithValue("@VacutainerId", dp.VacutainerId);
            com.Parameters.AddWithValue("@ReportTime", dp.ReportTime);
            com.Parameters.AddWithValue("@InvCode", dp.InvCode);
            com.Parameters.AddWithValue("@GuideLines", dp.GuideLines);
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
        public List<InvMasters> Get_InvestigationById(int Id)
        {
            List<InvMasters> inv = new List<InvMasters>();
            SqlCommand com = new SqlCommand("Usp_InvMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetInvestigationById");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                inv.Add(new InvMasters
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    InvName = rdr["InvName"].ToString(),
                    DepartmentName = rdr["DepartmentName"].ToString(),
                    SubDeptName = rdr["SubDeptName"].ToString(),
                    SpecimenName = rdr["SpecimenName"].ToString(),
                    VacutainerName = rdr["VacutainerName"].ToString(),
                    ReportHdr = rdr["ReportHdr"].ToString(),
                    Rate = Convert.ToDecimal(rdr["Rate"]),
                    DeptId = Convert.ToInt32(rdr["DeptId"]),
                    SubDeptId = Convert.ToInt32(rdr["SubDeptId"]),
                    SpecimenId = Convert.ToInt32(rdr["SpecimenId"]),
                    VacutainerId = Convert.ToInt32(rdr["VacutainerId"]),
                    ReportTime = rdr["ReportTime"].ToString(),
                    InvCode = rdr["InvCode"].ToString(),
                    GuideLines = rdr["GuideLines"].ToString(),
                    Active = Convert.ToBoolean(rdr["Active"])

                });
            }
            return inv;
        }

        public int Delete_Investigation(int Id)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_InvMaster", con);
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