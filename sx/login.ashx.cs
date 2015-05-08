using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;

namespace SX
{

    public class Login : IHttpHandler
    {

        public void ProcessRequest(HttpContext Context)
        {
            LoginRequest Request; LoginResponse Response = null;
            try
            {
                if (!LoginRequest.TryParse(Context, out Request)) throw new InvalidCastException("asql:401:Invalid login request.");
                using (SqlConnection Connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["Database"].ConnectionString))
                {
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand("apiUserLogin", Connection))
                    {
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("Email", Request.Email);
                        using (SqlDataReader Reader = Command.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (Reader.HasRows)
                                if (Reader.Read())
                                    if (Security.VerifyPassword(Request.Password, Reader.GetString(1)))
                                        Response = new LoginResponse(Reader.GetInt32(0));
                        }
                    }
                    if (Response == null) throw new UnauthorizedAccessException("asql:401:Invalid email address or password.");
                    Security.VerifyUser(Response.Token, true);
                    Connection.Close();
                }
                Context.Response.ContentType = "text/json";
                Context.Response.Write(JsonConvert.SerializeObject(Response, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception Exception) { Logging.LogError(Context, null, Exception); }
        }

        public bool IsReusable { get { return false; } }

    }
}