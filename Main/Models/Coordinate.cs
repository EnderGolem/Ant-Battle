using Newtonsoft.Json;

namespace Server.Net.Models;

public class Coordinate
{
    public Coordinate()
    {
    }

    public Coordinate(int q, int r)
    {
        Q = q;
        R = r;
    }

    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }

    public bool Equals(Coordinate other)
    {
        return (other.Q == Q) && (other.R == R);
    }
}
