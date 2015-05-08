using System;
using System.Web;

namespace SX
{

    public class Encrypt : IHttpHandler
    {

        public void ProcessRequest(HttpContext Context)
        {
            Context.Response.ContentType = "text/plain";
            if (Context.Request.QueryString["pw"] != null)
            {
                string Password = Context.Request.QueryString["pw"];
                string Encrypted = Security.EncryptPassword(Password);
                bool Match = Security.VerifyPassword(Password, Encrypted);
                Context.Response.Write(string.Format("{0}\r\n{1}", Encrypted, Match));
            }
            else Logging.LogError(Context, null, new InvalidOperationException("asql:400:Nothing to encrypt."));
        }

        public bool IsReusable { get { return false; } }

    }

}