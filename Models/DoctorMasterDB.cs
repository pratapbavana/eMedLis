using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.Models
{
    public class DoctorMasterDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);

        public List<DoctorMaster> Get_DoctorList()
        {
            var list = new List<DoctorMaster>();
            SqlCommand com = new SqlCommand("Usp_DoctorMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetList");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new DoctorMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        UserId = Convert.ToInt32(rdr["UserId"]),
                        UserName = Convert.ToString(rdr["UserName"]),
                        FullName = Convert.ToString(rdr["FullName"]),
                        Designation = Convert.ToString(rdr["Designation"]),
                        RegistrationNumber = Convert.ToString(rdr["RegistrationNumber"]),
                        SubDepartments = Convert.ToString(rdr["SubDepartments"]),
                        Active = Convert.ToBoolean(rdr["Active"]),
                        HasSignature = Convert.ToBoolean(rdr["HasSignature"])
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

        public List<DoctorMaster> Get_DoctorById(int id)
        {
            var list = new List<DoctorMaster>();
            SqlCommand com = new SqlCommand("Usp_DoctorMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", id);
            com.Parameters.AddWithValue("@Action", "GetById");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new DoctorMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        UserId = Convert.ToInt32(rdr["UserId"]),
                        UserName = Convert.ToString(rdr["UserName"]),
                        FullName = Convert.ToString(rdr["FullName"]),
                        Designation = Convert.ToString(rdr["Designation"]),
                        RegistrationNumber = Convert.ToString(rdr["RegistrationNumber"]),
                        SubDepartmentIds = Convert.ToString(rdr["SubDepartmentIds"]),
                        Active = Convert.ToBoolean(rdr["Active"]),
                        HasSignature = Convert.ToBoolean(rdr["HasSignature"])
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

        public List<DoctorMaster> Get_ActiveUsers()
        {
            var list = new List<DoctorMaster>();
            SqlCommand com = new SqlCommand("Usp_DoctorMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetUsers");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new DoctorMaster
                    {
                        UserId = Convert.ToInt32(rdr["UserId"]),
                        UserName = Convert.ToString(rdr["UserName"]),
                        FullName = Convert.ToString(rdr["FullName"])
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

        public Tuple<int, string> Add_Doctor(DoctorMaster doc, byte[] signatureBytes, string signatureMimeType)
        {
            return SaveDoctor(doc, signatureBytes, signatureMimeType, "Add");
        }

        public Tuple<int, string> Update_Doctor(DoctorMaster doc, byte[] signatureBytes, string signatureMimeType)
        {
            return SaveDoctor(doc, signatureBytes, signatureMimeType, "Update");
        }

        public Tuple<int, string> Set_DoctorActive(int id, bool active)
        {
            int statusCode = 0;
            string statusMsg = "";
            SqlCommand com = new SqlCommand("Usp_DoctorMaster", con);
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

        public Tuple<byte[], string> Get_Signature(int id)
        {
            SqlCommand com = new SqlCommand("Usp_DoctorMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", id);
            com.Parameters.AddWithValue("@Action", "GetSignature");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                if (rdr.Read())
                {
                    byte[] img = rdr["SignatureImage"] == DBNull.Value ? null : (byte[])rdr["SignatureImage"];
                    string mime = rdr["SignatureMimeType"] == DBNull.Value ? null : Convert.ToString(rdr["SignatureMimeType"]);
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

        private Tuple<int, string> SaveDoctor(DoctorMaster doc, byte[] signatureBytes, string signatureMimeType, string action)
        {
            int statusCode = 0;
            string statusMsg = "";
            SqlCommand com = new SqlCommand("Usp_DoctorMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", doc.Id);
            com.Parameters.AddWithValue("@UserId", doc.UserId);
            com.Parameters.AddWithValue("@Designation", (object)doc.Designation ?? DBNull.Value);
            com.Parameters.AddWithValue("@RegistrationNumber", (object)doc.RegistrationNumber ?? DBNull.Value);
            com.Parameters.AddWithValue("@SubDepartmentIds", (object)doc.SubDepartmentIds ?? DBNull.Value);
            var signatureImageParam = com.Parameters.Add("@SignatureImage", SqlDbType.VarBinary, -1);
            signatureImageParam.Value = (object)signatureBytes ?? DBNull.Value;
            var signatureMimeTypeParam = com.Parameters.Add("@SignatureMimeType", SqlDbType.VarChar, 50);
            signatureMimeTypeParam.Value = (object)signatureMimeType ?? DBNull.Value;
            com.Parameters.AddWithValue("@Active", doc.Active);
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
