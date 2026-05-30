using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ReferenceRangeDB
    {
        private static readonly ConcurrentDictionary<string, object> BatchLocks = new ConcurrentDictionary<string, object>();
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);
        private int StatusCode;
        private string StatusMsg = "";

        public Tuple<int, string> Add_ReferenceRange(ReferenceRange range)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", range.Id);
            com.Parameters.AddWithValue("@ParameterId", range.ParameterId);
            com.Parameters.AddWithValue("@MethodId", range.MethodId);
            com.Parameters.AddWithValue("@Gender", range.Gender);
            com.Parameters.AddWithValue("@AgeFromValue", range.AgeFromValue);
            com.Parameters.AddWithValue("@AgeFromUnit", range.AgeFromUnit);
            com.Parameters.AddWithValue("@AgeToValue", range.AgeToValue);
            com.Parameters.AddWithValue("@AgeToUnit", range.AgeToUnit);
            com.Parameters.AddWithValue("@AgeFromDays", range.AgeFromDays);
            com.Parameters.AddWithValue("@AgeToDays", range.AgeToDays);
            com.Parameters.AddWithValue("@NormalMin", (object)range.NormalMin ?? DBNull.Value);
            com.Parameters.AddWithValue("@NormalMax", (object)range.NormalMax ?? DBNull.Value);
            com.Parameters.AddWithValue("@CriticalMin", (object)range.CriticalMin ?? DBNull.Value);
            com.Parameters.AddWithValue("@CriticalMax", (object)range.CriticalMax ?? DBNull.Value);
            com.Parameters.AddWithValue("@RangeText", (object)range.RangeText ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", range.Active);
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

        public Tuple<int, string> Add_ReferenceRangeBatch(ReferenceRange range)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", range.Id);
            com.Parameters.AddWithValue("@ParameterId", range.ParameterId);
            com.Parameters.AddWithValue("@MethodId", range.MethodId);
            com.Parameters.AddWithValue("@Gender", range.Gender);
            com.Parameters.AddWithValue("@AgeFromValue", range.AgeFromValue);
            com.Parameters.AddWithValue("@AgeFromUnit", range.AgeFromUnit);
            com.Parameters.AddWithValue("@AgeToValue", range.AgeToValue);
            com.Parameters.AddWithValue("@AgeToUnit", range.AgeToUnit);
            com.Parameters.AddWithValue("@AgeFromDays", range.AgeFromDays);
            com.Parameters.AddWithValue("@AgeToDays", range.AgeToDays);
            com.Parameters.AddWithValue("@NormalMin", (object)range.NormalMin ?? DBNull.Value);
            com.Parameters.AddWithValue("@NormalMax", (object)range.NormalMax ?? DBNull.Value);
            com.Parameters.AddWithValue("@CriticalMin", (object)range.CriticalMin ?? DBNull.Value);
            com.Parameters.AddWithValue("@CriticalMax", (object)range.CriticalMax ?? DBNull.Value);
            com.Parameters.AddWithValue("@RangeText", (object)range.RangeText ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", range.Active);
            com.Parameters.AddWithValue("@Action", "AddBatch");
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

        public Tuple<int, string> Update_ReferenceRange(ReferenceRange range)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", range.Id);
            com.Parameters.AddWithValue("@ParameterId", range.ParameterId);
            com.Parameters.AddWithValue("@MethodId", range.MethodId);
            com.Parameters.AddWithValue("@Gender", range.Gender);
            com.Parameters.AddWithValue("@AgeFromValue", range.AgeFromValue);
            com.Parameters.AddWithValue("@AgeFromUnit", range.AgeFromUnit);
            com.Parameters.AddWithValue("@AgeToValue", range.AgeToValue);
            com.Parameters.AddWithValue("@AgeToUnit", range.AgeToUnit);
            com.Parameters.AddWithValue("@AgeFromDays", range.AgeFromDays);
            com.Parameters.AddWithValue("@AgeToDays", range.AgeToDays);
            com.Parameters.AddWithValue("@NormalMin", (object)range.NormalMin ?? DBNull.Value);
            com.Parameters.AddWithValue("@NormalMax", (object)range.NormalMax ?? DBNull.Value);
            com.Parameters.AddWithValue("@CriticalMin", (object)range.CriticalMin ?? DBNull.Value);
            com.Parameters.AddWithValue("@CriticalMax", (object)range.CriticalMax ?? DBNull.Value);
            com.Parameters.AddWithValue("@RangeText", (object)range.RangeText ?? DBNull.Value);
            com.Parameters.AddWithValue("@Active", range.Active);
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

        public List<ReferenceRange> Get_ReferenceRangesByParameter(int ParameterId)
        {
            List<ReferenceRange> ranges = new List<ReferenceRange>();
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ParameterId", ParameterId);
            com.Parameters.AddWithValue("@Action", "GetByParameter");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                ranges.Add(new ReferenceRange
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    ParameterId = Convert.ToInt32(rdr["ParameterId"]),
                    ParameterName = rdr["ParameterName"].ToString(),
                    MethodId = Convert.ToInt32(rdr["MethodId"]),
                    Gender = rdr["Gender"].ToString(),
                    MethodName = rdr["MethodName"].ToString(),
                    AgeFromValue = Convert.ToDecimal(rdr["AgeFromValue"]),
                    AgeFromUnit = rdr["AgeFromUnit"].ToString(),
                    AgeToValue = Convert.ToDecimal(rdr["AgeToValue"]),
                    AgeToUnit = rdr["AgeToUnit"].ToString(),
                    AgeFromDays = Convert.ToInt32(rdr["AgeFromDays"]),
                    AgeToDays = Convert.ToInt32(rdr["AgeToDays"]),
                    NormalMin = rdr["NormalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMin"]),
                    NormalMax = rdr["NormalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMax"]),
                    CriticalMin = rdr["CriticalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMin"]),
                    CriticalMax = rdr["CriticalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMax"]),
                    RangeText = rdr["RangeText"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["RangeText"]),
                    Active = Convert.ToBoolean(rdr["Active"])
                });
            }
            return ranges;
        }

        public List<ReferenceRange> Get_ReferenceRangeById(int Id)
        {
            List<ReferenceRange> ranges = new List<ReferenceRange>();
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Action", "GetById");
            con.Open();
            SqlDataReader rdr = com.ExecuteReader();
            while (rdr.Read())
            {
                ranges.Add(new ReferenceRange
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    ParameterId = Convert.ToInt32(rdr["ParameterId"]),
                    MethodId = Convert.ToInt32(rdr["MethodId"]),
                    Gender = rdr["Gender"].ToString(),
                    MethodName = rdr["MethodName"].ToString(),
                    AgeFromValue = Convert.ToDecimal(rdr["AgeFromValue"]),
                    AgeFromUnit = rdr["AgeFromUnit"].ToString(),
                    AgeToValue = Convert.ToDecimal(rdr["AgeToValue"]),
                    AgeToUnit = rdr["AgeToUnit"].ToString(),
                    AgeFromDays = Convert.ToInt32(rdr["AgeFromDays"]),
                    AgeToDays = Convert.ToInt32(rdr["AgeToDays"]),
                    NormalMin = rdr["NormalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMin"]),
                    NormalMax = rdr["NormalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMax"]),
                    CriticalMin = rdr["CriticalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMin"]),
                    CriticalMax = rdr["CriticalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMax"]),
                    RangeText = rdr["RangeText"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["RangeText"]),
                    Active = Convert.ToBoolean(rdr["Active"])
                });
            }
            return ranges;
        }

        public Tuple<int, string> Set_ReferenceRangeActive(int Id, bool Active)
        {
            int i;
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Id", Id);
            com.Parameters.AddWithValue("@Active", Active);
            com.Parameters.AddWithValue("@Action", "SetActive");
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

        public List<ReferenceRangeCombo> Get_ReferenceRangeCombos()
        {
            List<ReferenceRangeCombo> combos = new List<ReferenceRangeCombo>();
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Action", "GetCombos");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    combos.Add(new ReferenceRangeCombo
                    {
                        ParameterId = Convert.ToInt32(rdr["ParameterId"]),
                        ParameterName = rdr["ParameterName"].ToString(),
                        MethodId = Convert.ToInt32(rdr["MethodId"]),
                        MethodName = rdr["MethodName"].ToString(),
                        RangeCount = Convert.ToInt32(rdr["RangeCount"]),
                        ActiveCount = Convert.ToInt32(rdr["ActiveCount"])
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
            return combos;
        }

        public List<ReferenceRange> Get_ReferenceRangesByParameterMethod(int ParameterId, int MethodId)
        {
            List<ReferenceRange> ranges = new List<ReferenceRange>();
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ParameterId", ParameterId);
            com.Parameters.AddWithValue("@MethodId", MethodId);
            com.Parameters.AddWithValue("@Action", "GetByParameterMethod");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    ranges.Add(new ReferenceRange
                    {
                    Id = Convert.ToInt32(rdr["Id"]),
                    ParameterId = Convert.ToInt32(rdr["ParameterId"]),
                    ParameterName = rdr["ParameterName"].ToString(),
                    MethodId = Convert.ToInt32(rdr["MethodId"]),
                    Gender = rdr["Gender"].ToString(),
                    MethodName = rdr["MethodName"].ToString(),
                        AgeFromValue = Convert.ToDecimal(rdr["AgeFromValue"]),
                        AgeFromUnit = rdr["AgeFromUnit"].ToString(),
                        AgeToValue = Convert.ToDecimal(rdr["AgeToValue"]),
                        AgeToUnit = rdr["AgeToUnit"].ToString(),
                        AgeFromDays = Convert.ToInt32(rdr["AgeFromDays"]),
                        AgeToDays = Convert.ToInt32(rdr["AgeToDays"]),
                        NormalMin = rdr["NormalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMin"]),
                        NormalMax = rdr["NormalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["NormalMax"]),
                        CriticalMin = rdr["CriticalMin"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMin"]),
                        CriticalMax = rdr["CriticalMax"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["CriticalMax"]),
                        RangeText = rdr["RangeText"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["RangeText"]),
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
            return ranges;
        }

        public bool Exists_ReferenceRangeCombo(int ParameterId, int MethodId)
        {
            bool exists = false;
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ParameterId", ParameterId);
            com.Parameters.AddWithValue("@MethodId", MethodId);
            com.Parameters.AddWithValue("@Action", "ExistsByParamMethod");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                if (rdr.Read())
                {
                    exists = Convert.ToInt32(rdr["ExistsFlag"]) == 1;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }
            return exists;
        }

        public void Delete_ReferenceRangesByParameterMethod(int ParameterId, int MethodId)
        {
            SqlCommand com = new SqlCommand("Usp_ReferenceRange", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@ParameterId", ParameterId);
            com.Parameters.AddWithValue("@MethodId", MethodId);
            com.Parameters.AddWithValue("@Action", "DeleteByParamMethod");
            try
            {
                con.Open();
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }
        }

        public Tuple<int, string> Replace_ReferenceRangeBatch(int parameterId, int methodId, List<ReferenceRange> ranges)
        {
            if (parameterId <= 0 || methodId <= 0)
            {
                return new Tuple<int, string>(0, "Invalid parameter/method");
            }
            if (ranges == null || ranges.Count == 0)
            {
                return new Tuple<int, string>(0, "At least one range is required");
            }

            var lockKey = parameterId.ToString() + "|" + methodId.ToString();
            var lockObj = BatchLocks.GetOrAdd(lockKey, new object());

            lock (lockObj)
            {
                using (var conn = new SqlConnection(con.ConnectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var del = new SqlCommand("Usp_ReferenceRange", conn, tran))
                            {
                                del.CommandType = CommandType.StoredProcedure;
                                del.Parameters.AddWithValue("@ParameterId", parameterId);
                                del.Parameters.AddWithValue("@MethodId", methodId);
                                del.Parameters.AddWithValue("@Action", "DeleteByParamMethod");
                                del.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                                del.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                                del.ExecuteNonQuery();
                            }

                            foreach (var range in ranges)
                            {
                                using (var ins = new SqlCommand("Usp_ReferenceRange", conn, tran))
                                {
                                    ins.CommandType = CommandType.StoredProcedure;
                                    ins.Parameters.AddWithValue("@ParameterId", parameterId);
                                    ins.Parameters.AddWithValue("@MethodId", methodId);
                                    ins.Parameters.AddWithValue("@Gender", range.Gender);
                                    ins.Parameters.AddWithValue("@AgeFromValue", range.AgeFromValue);
                                    ins.Parameters.AddWithValue("@AgeFromUnit", range.AgeFromUnit);
                                    ins.Parameters.AddWithValue("@AgeToValue", range.AgeToValue);
                                    ins.Parameters.AddWithValue("@AgeToUnit", range.AgeToUnit);
                                    ins.Parameters.AddWithValue("@AgeFromDays", range.AgeFromDays);
                                    ins.Parameters.AddWithValue("@AgeToDays", range.AgeToDays);
                                    ins.Parameters.AddWithValue("@NormalMin", (object)range.NormalMin ?? DBNull.Value);
                                    ins.Parameters.AddWithValue("@NormalMax", (object)range.NormalMax ?? DBNull.Value);
                                    ins.Parameters.AddWithValue("@CriticalMin", (object)range.CriticalMin ?? DBNull.Value);
                                    ins.Parameters.AddWithValue("@CriticalMax", (object)range.CriticalMax ?? DBNull.Value);
                                    ins.Parameters.AddWithValue("@RangeText", (object)range.RangeText ?? DBNull.Value);
                                    ins.Parameters.AddWithValue("@Active", range.Active);
                                    ins.Parameters.AddWithValue("@Action", "AddBatch");
                                    ins.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                                    ins.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                                    ins.ExecuteNonQuery();

                                    var sc = Convert.ToInt32(ins.Parameters["@StatusCode"].Value);
                                    var sm = Convert.ToString(ins.Parameters["@StatusMsg"].Value);
                                    if (sc != 1)
                                    {
                                        tran.Rollback();
                                        return new Tuple<int, string>(0, string.IsNullOrWhiteSpace(sm) ? "Failed to save ranges" : sm);
                                    }
                                }
                            }

                            tran.Commit();
                            return new Tuple<int, string>(1, "Reference Ranges Saved Successfully");
                        }
                        catch (Exception ex)
                        {
                            try { tran.Rollback(); } catch { }
                            return new Tuple<int, string>(0, ex.Message);
                        }
                    }
                }
            }
        }
    }
}
