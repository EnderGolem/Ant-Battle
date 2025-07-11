using Newtonsoft.Json;

namespace Server.Net.Models;

public class Message : Base
{
    [JsonProperty("time")]
    public string Time { get; set; } = string.Empty;
}
