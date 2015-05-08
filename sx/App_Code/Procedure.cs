using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace SX
{

    public class Procedure
    {

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Parameters")]
        public List<Parameter> Parameters { get; set; }

        [JsonProperty("UserId")]
        public bool UserId { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        public static bool TryParse(HttpContext Context, out Procedure Result)
        {
            Result = null;
            try
            {
                using (StreamReader Reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8))
                {
                    Result = JsonConvert.DeserializeObject<Procedure>(Reader.ReadToEnd());
                    return true;
                }
            }
            catch { return false; }
        }

    }

}