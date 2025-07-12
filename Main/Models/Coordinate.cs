using Newtonsoft.Json;

namespace Server.Net.Models;

public class Coordinate
{
    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }

    public bool Equals(Coordinate other)
    {
        return (other.Q == Q) && (other.R == R);
    }
}
