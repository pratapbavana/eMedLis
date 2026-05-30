using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eMedLis.Models
{
    public class ParameterMasterDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        private int StatusCode;
        private string StatusMsg = "";

        public Tuple<int, string> Add_Parameter(ParameterMaster param)
        {
            SqlCommand com = new SqlCommand("Usp_ParameterMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", param.Id);
            com.Parameters.AddWithValue("@ParameterHeaderId", (object)param.ParameterHeaderId ?? DBNull.Value);
            com.Parameters.AddWithValue("@ParameterName", param.ParameterName);
            com.Parameters.AddWithValue("@ShortName", (object)param.ShortName ?? DBNull.Value);
            com.Parameters.AddWithValue("@Unit", (object)param.Unit ?? DBNull.Value);
            com.Parameters.AddWithValue("@ResultType", param.ResultType);
            com.Parameters.AddWithValue("@DecimalPrecision", param.DecimalPrecision);
            com.Parameters.AddWithValue("@AllowRange", param.AllowRange);
            com.Parameters.AddWithValue("@AllowCriticalRange", param.AllowCriticalRange);
            com.Parameters.AddWithValue("@IsCalculated", param.IsCalculated);
            com.Parameters.AddWithValue("@Formula", (object)param.Formula ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", param.Active);
            com.Parameters.AddWithValue("@Action", "Add");
            com.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                com.ExecuteNonQuery();
                StatusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                StatusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<ParameterMaster> Get_Parameter()
        {
            List<ParameterMaster> parameters = new List<ParameterMaster>();
            SqlCommand com = new SqlCommand("Usp_ParameterMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetParameter");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    parameters.Add(new ParameterMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        ParameterHeaderId = rdr["ParameterHeaderId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ParameterHeaderId"]),
                        ParameterHeaderName = HasColumn(rdr, "HeaderName") ? Convert.ToString(rdr["HeaderName"]) : string.Empty,
                        ParameterName = Convert.ToString(rdr["ParameterName"]),
                        ShortName = Convert.ToString(rdr["ShortName"]),
                        Unit = Convert.ToString(rdr["Unit"]),
                        ResultType = Convert.ToString(rdr["ResultType"]),
                        DecimalPrecision = Convert.ToInt32(rdr["DecimalPrecision"]),
                        AllowRange = Convert.ToBoolean(rdr["AllowRange"]),
                        AllowCriticalRange = Convert.ToBoolean(rdr["AllowCriticalRange"]),
                        IsCalculated = Convert.ToBoolean(rdr["IsCalculated"]),
                        Formula = Convert.ToString(rdr["Formula"]),
                        DropdownDisplayValues = HasColumn(rdr, "DropdownDisplayValues") ? Convert.ToString(rdr["DropdownDisplayValues"]) : string.Empty,
                        Active = Convert.ToBoolean(rdr["Active"])
                    });
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }

            return parameters;
        }

        public Tuple<int, string> Update_Parameter(ParameterMaster param)
        {
            SqlCommand com = new SqlCommand("Usp_ParameterMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", param.Id);
            com.Parameters.AddWithValue("@ParameterHeaderId", (object)param.ParameterHeaderId ?? DBNull.Value);
            com.Parameters.AddWithValue("@ParameterName", param.ParameterName);
            com.Parameters.AddWithValue("@ShortName", (object)param.ShortName ?? DBNull.Value);
            com.Parameters.AddWithValue("@Unit", (object)param.Unit ?? DBNull.Value);
            com.Parameters.AddWithValue("@ResultType", param.ResultType);
            com.Parameters.AddWithValue("@DecimalPrecision", param.DecimalPrecision);
            com.Parameters.AddWithValue("@AllowRange", param.AllowRange);
            com.Parameters.AddWithValue("@AllowCriticalRange", param.AllowCriticalRange);
            com.Parameters.AddWithValue("@IsCalculated", param.IsCalculated);
            com.Parameters.AddWithValue("@Formula", (object)param.Formula ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", param.Active);
            com.Parameters.AddWithValue("@Action", "Update");
            com.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                com.ExecuteNonQuery();
                StatusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                StatusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<ParameterMaster> Get_ParameterById(int Id)
        {
            List<ParameterMaster> parameters = new List<ParameterMaster>();
            SqlCommand com = new SqlCommand("Usp_ParameterMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetParameterById");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    parameters.Add(new ParameterMaster
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        ParameterHeaderId = rdr["ParameterHeaderId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ParameterHeaderId"]),
                        ParameterName = Convert.ToString(rdr["ParameterName"]),
                        ShortName = Convert.ToString(rdr["ShortName"]),
                        Unit = Convert.ToString(rdr["Unit"]),
                        ResultType = Convert.ToString(rdr["ResultType"]),
                        DecimalPrecision = Convert.ToInt32(rdr["DecimalPrecision"]),
                        AllowRange = Convert.ToBoolean(rdr["AllowRange"]),
                        AllowCriticalRange = Convert.ToBoolean(rdr["AllowCriticalRange"]),
                        IsCalculated = Convert.ToBoolean(rdr["IsCalculated"]),
                        Formula = Convert.ToString(rdr["Formula"]),
                        Active = Convert.ToBoolean(rdr["Active"])
                    });
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }

            return parameters;
        }

        public Tuple<int, string> Set_ParameterActive(int Id, bool Active)
        {
            SqlCommand com = new SqlCommand("Usp_ParameterMaster", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Active", Active);
            com.Parameters.AddWithValue("@Action", "SetActive");
            com.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                com.ExecuteNonQuery();
                StatusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                StatusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(StatusCode, StatusMsg);
        }

        public List<ParameterDropdownValue> Get_DropdownValues(int parameterId)
        {
            var list = new List<ParameterDropdownValue>();
            SqlCommand com = new SqlCommand("Usp_ParameterDropdownValue", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ParameterId", parameterId);
            com.Parameters.AddWithValue("@Action", "GetByParameter");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new ParameterDropdownValue
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        ParameterId = Convert.ToInt32(rdr["ParameterId"]),
                        ValueText = Convert.ToString(rdr["ValueText"]),
                        DisplayOrder = Convert.ToInt32(rdr["DisplayOrder"]),
                        Active = Convert.ToBoolean(rdr["Active"])
                    });
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }

            return list;
        }

        public Tuple<int, string> Save_DropdownValues(int parameterId, List<ParameterDropdownValue> values)
        {
            SqlCommand del = new SqlCommand("Usp_ParameterDropdownValue", con);
            del.CommandType = CommandType.StoredProcedure;
            del.Parameters.AddWithValue("@ParameterId", parameterId);
            del.Parameters.AddWithValue("@Action", "DeleteByParameter");

            try
            {
                con.Open();
                del.ExecuteNonQuery();

                if (values != null)
                {
                    foreach (var value in values)
                    {
                        SqlCommand ins = new SqlCommand("Usp_ParameterDropdownValue", con);
                        ins.CommandType = CommandType.StoredProcedure;
                        ins.Parameters.AddWithValue("@ParameterId", parameterId);
                        ins.Parameters.AddWithValue("@ValueText", value.ValueText);
                        ins.Parameters.AddWithValue("@DisplayOrder", value.DisplayOrder);
                        ins.Parameters.AddWithValue("@Active", true);
                        ins.Parameters.AddWithValue("@Action", "Add");
                        ins.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return new Tuple<int, string>(0, ex.Message);
            }
            finally
            {
                con.Close();
            }

            return new Tuple<int, string>(1, "Dropdown values saved successfully");
        }

        private static bool HasColumn(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
