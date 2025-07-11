using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.Net.Models

{
    public class MovesRequest : Base
    {
        [JsonProperty("moves")]
        public IList<Move> Moves { get; set; } = new List<Move>();
    }

    public class Move
    {
        [JsonProperty("ant")]
        public string Ant { get; set; } = string.Empty;

        [JsonProperty("path")]
        public IList<Coordinate> Path { get; set; } = new List<Coordinate>();
    }
}