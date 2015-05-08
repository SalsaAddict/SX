using Newtonsoft.Json;

namespace SX
{

    public class Parameter
    {

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Value")]
        public object Value { get; set; }

        [JsonProperty("XML")]
        public bool XML { get; set; }

    }

}