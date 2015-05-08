using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace SX
{

    public static class Logging
    {

        public static readonly string UnexpectedError = "An unexpected error occurred.";
        public static readonly string LoggingError = "Unable to log error message.";

        public static void LogError(HttpContext Context, Procedure Procedure, Exception Exception)
        {

            int StatusCode; string Message;
            if (Exception.Message.StartsWith("asql:"))
            {
                string[] Split = Exception.Message.Split(":".ToCharArray());
                try { StatusCode = Convert.ToInt32(Split[1]); }
                catch { StatusCode = 500; }
                Message = Split[2];
            }
            else if (Exception.Message.Contains("Cannot insert duplicate key"))
            {
                StatusCode = 400;
                Message = "A similar record already exists.";
            }
            else { StatusCode = 500; Message = UnexpectedError; }

            string IPAddress = Context.Request.UserHostAddress;
            if (string.IsNullOrWhiteSpace(IPAddress)) IPAddress = "Unknown";

            string RequestBody;
            try { using (StreamReader Reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8)) { RequestBody = Reader.ReadToEnd(); } }
            catch { RequestBody = null; }
            if (string.IsNullOrWhiteSpace(RequestBody)) RequestBody = null;

            string ProcedureXML;
            try { ProcedureXML = JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(Procedure), "Procedure").InnerXml; }
            catch { ProcedureXML = null; }
            if (string.IsNullOrWhiteSpace(ProcedureXML)) ProcedureXML = null;

            try
            {
                using (SqlConnection Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["Database"].ConnectionString))
                {
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand("apiErrorLog", Connection))
                    {
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("IPAddress", IPAddress);
                        Command.Parameters.AddWithValue("URL", Context.Request.Path);
                        Command.Parameters.AddWithValue("QueryString", Context.Request.QueryString.ToString());
                        Command.Parameters.AddWithValue("RequestBody", RequestBody);
                        Command.Parameters.AddWithValue("Procedure", ProcedureXML);
                        Command.Parameters.AddWithValue("Exception", Exception.Message);
                        Command.Parameters.AddWithValue("Message", Message);
                        Command.Parameters.AddWithValue("StackTrace", new StackTrace(Exception, true).ToString());
                        Command.ExecuteNonQuery();
                    }
                    Connection.Close();
                }
            }
            catch (Exception Ex) { Message = Ex.Message; }

            Context.Response.Clear();
            Context.Response.ContentType = "text/plain";
            Context.Response.Write(Message);
            Context.Response.StatusCode = StatusCode;
            Context.Response.End();

        }

    }

}