using Newtonsoft.Json;

namespace Server.Net.Models;


public class GameState : Base
{
    [JsonProperty("ants")]
    public IList<Ant> Ants { get; set; } = new List<Ant>();

    [JsonProperty("enemies")]
    public IList<Enemy> Enemies { get; set; } = new List<Enemy>();

    [JsonProperty("food")]
    public IList<FoodOnMap> Food { get; set; } = new List<FoodOnMap>();

    [JsonProperty("home")]
    public IList<Coordinate> Home { get; set; } = new List<Coordinate>();

    [JsonProperty("map")]
    public IList<MapTile> Map { get; set; } = new List<MapTile>();

    [JsonProperty("nextTurnIn")]
    public decimal NextTurnIn { get; set; }

    [JsonProperty("score")]
    public decimal Score { get; set; }

    [JsonProperty("spot")]
    public Coordinate Spot { get; set; } = new Coordinate();

    [JsonProperty("turnNo")]
    public int TurnNo { get; set; }
}

public class Ant
{
    [JsonProperty("food")]
    public AntFood Food { get; set; } = new AntFood();

    [JsonProperty("health")]
    public int Health { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("lastAttack")]
    public Coordinate LastAttack { get; set; } = new Coordinate();

    [JsonProperty("lastEnemyAnt")]
    public string LastEnemyAnt { get; set; } = string.Empty;

    [JsonProperty("lastMove")]
    public IList<Coordinate> LastMove { get; set; } = new List<Coordinate>();

    [JsonProperty("move")]
    public IList<Coordinate> Move { get; set; } = new List<Coordinate>();

    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }

    [JsonProperty("type")]
    public AntRole Type { get; set; }
}

public class AntFood
{
    [JsonProperty("amount")]
    public int Amount { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }
}

public class Enemy
{
    [JsonProperty("attack")]
    public int Attack { get; set; }

    [JsonProperty("food")]
    public AntFood Food { get; set; } = new AntFood();

    [JsonProperty("health")]
    public int Health { get; set; }

    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }

    [JsonProperty("type")]
    public AntRole Type { get; set; } //Тут не уверен какой тип данных
}

public class FoodOnMap
{
    [JsonProperty("amount")]
    public int Amount { get; set; }

    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }

    [JsonProperty("type")]
    public FoodType Type { get; set; }
}

public class MapTile
{
    [JsonProperty("cost")]
    public int Cost { get; set; }

    [JsonProperty("q")]
    public int Q { get; set; }

    [JsonProperty("r")]
    public int R { get; set; }

    [JsonProperty("type")]
    public HexType Type { get; set; }
}


public enum HexType
{
    Anthill = 1,
    Empty = 2,
    Dirt = 3,
    Acid = 4,
    Rocks = 5
}
public enum FoodType
{
    Apple = 1,
    Bread = 2,
    Nectar = 3
}

public enum AntRole
{
    Worker = 0,
    Fighter = 1,
    Scout = 2
}