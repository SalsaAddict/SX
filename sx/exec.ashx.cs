using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Xml;
using System.IO;
using System.Text;

namespace SX
{

    public class Exec : IHttpHandler
    {

        public void ProcessRequest(HttpContext Context)
        {
            Context.Response.ContentType = "text/json";
            Procedure Procedure = null;
            try
            {
                if (!Procedure.TryParse(Context, out Procedure)) throw new InvalidOperationException("asql:400:Invalid request.");
                if (string.IsNullOrWhiteSpace(Procedure.Token)) throw new UnauthorizedAccessException("asql:401:You must login to continue.");
                Security.VerifyUser(Procedure.Token);
                using (SqlConnection Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["Database"].ConnectionString))
                {
                    Connection.Open();
                    using (SqlTransaction Transaction = Connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            using (SqlCommand Command = new SqlCommand(Procedure.Name, Connection, Transaction))
                            {
                                Command.CommandType = CommandType.StoredProcedure;
                                foreach (Parameter Parameter in Procedure.Parameters)
                                {
                                    if (Parameter.XML)
                                        Command.Parameters.AddWithValue(Parameter.Name, JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(Parameter.Value), "Object").InnerXml);
                                    else
                                        Command.Parameters.AddWithValue(Parameter.Name, Parameter.Value);
                                }
                                if (Procedure.UserId) Command.Parameters.AddWithValue("UserId", Security.UserIdFromToken(Procedure.Token));
                                if (Procedure.Type == "execute") Command.ExecuteNonQuery();
                                else if (Procedure.Type == "object")
                                {
                                    using (XmlReader Reader = Command.ExecuteXmlReader())
                                    {
                                        XmlDocument Document = new XmlDocument();
                                        Document.Load(Reader);
                                        Context.Response.Write(JsonConvert.SerializeXmlNode(Document, Newtonsoft.Json.Formatting.Indented));
                                    }
                                }
                                else
                                {
                                    using (SqlDataReader Reader = Command.ExecuteReader((Procedure.Type == "singleton") ? CommandBehavior.SingleRow : CommandBehavior.SingleResult))
                                    {
                                        using (DataTable Table = new DataTable())
                                        {
                                            Table.Load(Reader);
                                            Context.Response.Write(JsonConvert.SerializeObject(Table, Newtonsoft.Json.Formatting.Indented));
                                        }
                                    }
                                }
                                Transaction.Commit();
                            }
                        }
                        catch (Exception Exception) { Transaction.Rollback(); throw Exception; }
                    }
                    Connection.Close();
                }
            }
            catch (Exception Exception) { Logging.LogError(Context, Procedure, Exception); }
        }

        public bool IsReusable { get { return false; } }

    }

}