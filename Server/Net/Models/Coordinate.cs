using Newtonsoft.Json;

namespace Server.Net.Models;

public class Coordinate
{
    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }
}
