using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Web;

namespace SX
{

    public class LoginRequest
    {

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        public static bool TryParse(HttpContext Context, out LoginRequest Result)
        {
            Result = null;
            try
            {
                using (StreamReader Reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8))
                {
                    Result = JsonConvert.DeserializeObject<LoginRequest>(Reader.ReadToEnd());
                    return !(string.IsNullOrWhiteSpace(Result.Email) || string.IsNullOrWhiteSpace(Result.Password));
                }
            }
            catch { return false; }
        }

    }

}