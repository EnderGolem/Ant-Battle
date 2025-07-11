using Newtonsoft.Json;

namespace Server.Net.Models

{
    public class Base
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
