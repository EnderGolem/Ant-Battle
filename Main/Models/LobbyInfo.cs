using Newtonsoft.Json;

namespace Main.Models;

public class LobbyInfo
{
    [JsonProperty("lobbyEndsIn")]
    public int LobbyEndsIn { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("nextTurn")]
    public int NextTurn { get; set; }

    [JsonProperty("realm")]
    public string Realm { get; set; } = string.Empty;
}
