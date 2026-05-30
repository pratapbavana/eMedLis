using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class InvestigationTemplateDB
    {
        private readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString);

        public List<InvestigationTemplateItem> Get_TemplateByInvestigation(string investigationId)
        {
            List<InvestigationTemplateItem> items = new List<InvestigationTemplateItem>();
            SqlCommand com = new SqlCommand("Usp_InvestigationTemplate", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@InvestigationId", investigationId);
            com.Parameters.AddWithValue("@Action", "GetByInvestigation");
            try
            {
                con.Open();
                SqlDataReader rdr = com.ExecuteReader();
                while (rdr.Read())
                {
                    items.Add(new InvestigationTemplateItem
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        InvestigationId = rdr["InvestigationId"].ToString(),
                        InvestigationName = rdr["InvestigationName"].ToString(),
                        ItemType = rdr["ItemType"].ToString(),
                        HeaderId = rdr["HeaderId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["HeaderId"]),
                        HeaderName = rdr["HeaderName"].ToString(),
                        ParameterId = rdr["ParameterId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["ParameterId"]),
                        ParameterName = rdr["ParameterName"].ToString(),
                        MethodId = HasColumn(rdr, "MethodId") && rdr["MethodId"] != DBNull.Value ? (int?)Convert.ToInt32(rdr["MethodId"]) : null,
                        MethodName = HasColumn(rdr, "MethodName") ? rdr["MethodName"].ToString() : string.Empty,
                        DisplayOrder = Convert.ToInt32(rdr["DisplayOrder"]),
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
            return items;
        }

        public void Delete_TemplateByInvestigation(string investigationId)
        {
            SqlCommand com = new SqlCommand("Usp_InvestigationTemplate", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@InvestigationId", investigationId);
            com.Parameters.AddWithValue("@Action", "DeleteByInvestigation");
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

        public Tuple<int, string> Add_TemplateItem(InvestigationTemplateItem item)
        {
            int statusCode = 0;
            string statusMsg = "";
            SqlCommand com = new SqlCommand("Usp_InvestigationTemplate", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@InvestigationId", item.InvestigationId);
            com.Parameters.AddWithValue("@ItemType", item.ItemType);
            com.Parameters.AddWithValue("@HeaderId", (object)item.HeaderId ?? DBNull.Value);
            com.Parameters.AddWithValue("@ParameterId", (object)item.ParameterId ?? DBNull.Value);
            com.Parameters.AddWithValue("@MethodId", (object)item.MethodId ?? DBNull.Value);
            com.Parameters.AddWithValue("@DisplayOrder", item.DisplayOrder);
            com.Parameters.AddWithValue("@Active", item.Active);
            com.Parameters.AddWithValue("@Action", "AddItem");
            com.Parameters.Add("@StatusCode", SqlDbType.Int);
            com.Parameters["@StatusCode"].Direction = ParameterDirection.Output;
            com.Parameters.Add("@StatusMsg", SqlDbType.VarChar, 100);
            com.Parameters["@StatusMsg"].Direction = ParameterDirection.Output;
            try
            {
                con.Open();
                com.ExecuteNonQuery();
                statusCode = Convert.ToInt32(com.Parameters["@StatusCode"].Value);
                statusMsg = Convert.ToString(com.Parameters["@StatusMsg"].Value);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }
            return new Tuple<int, string>(statusCode, statusMsg);
        }

        public string Get_InterpretationByInvestigation(string investigationId)
        {
            string html = string.Empty;
            SqlCommand com = new SqlCommand("Usp_InvestigationTemplate", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@InvestigationId", investigationId);
            com.Parameters.AddWithValue("@Action", "GetInterpretation");
            try
            {
                con.Open();
                var result = com.ExecuteScalar();
                html = result == null || result == DBNull.Value ? string.Empty : Convert.ToString(result);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                con.Close();
            }

            return html;
        }

        public void Save_Interpretation(string investigationId, string interpretationHtml)
        {
            SqlCommand com = new SqlCommand("Usp_InvestigationTemplate", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@InvestigationId", investigationId);
            com.Parameters.AddWithValue("@InterpretationHtml", (object)interpretationHtml ?? DBNull.Value);
            com.Parameters.AddWithValue("@Action", "SaveInterpretation");
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
