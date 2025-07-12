using Newtonsoft.Json;

namespace Server.Net.Models;


public class GameState : Base
{
    [JsonProperty("ants")]
    public List<Ant> Ants { get; set; } = new List<Ant>();

    [JsonProperty("enemies")]
    public List<Enemy> Enemies { get; set; } = new List<Enemy>();

    [JsonProperty("food")]
    public List<FoodOnMap> Food { get; set; } = new List<FoodOnMap>();

    [JsonProperty("home")]
    public List<Coordinate> Home { get; set; } = new List<Coordinate>();

    [JsonProperty("map")]
    public List<MapTile> Map { get; set; } = new List<MapTile>();

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
    public AntType Type { get; set; }
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
    public AntType Type { get; set; } //Тут не уверен какой тип данных
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

public enum FoodType
{
    Apple = 1, Bread, Nectar
}

public enum AntType
{
    Worker = 0,
    Warrior,
    Scout
}

public enum HexType
{
    Base = 1,
    Default,
    Dirt,
    Acid,
    Obstacle,
    EndOfMap,
    EnemyBase,
    Fake
}

