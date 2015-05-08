using Newtonsoft.Json;

namespace SX
{

    public class LoginResponse
    {

        [JsonProperty("Token")]
        public string Token { get; private set; }

        [JsonProperty("Reset")]
        public bool Reset { get; private set; }

        public LoginResponse(int UserId, bool Reset = false)
        {
            this.Token = Security.TokenFromUserId(UserId);
            this.Reset = Reset;
        }

    }

}