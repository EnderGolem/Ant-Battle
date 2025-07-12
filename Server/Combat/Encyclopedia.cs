using Server.Net.Models;

namespace Server.Combat;

public static class Encyclopedia
{
    private static Dictionary<AntType, AntStats> _antStats;
    
    private static Dictionary<FoodType, FoodStats> _foodStats;

    static Encyclopedia()
    {
        _antStats = new Dictionary<AntType, AntStats>();
        _antStats[AntType.Worker] = new AntStats(AntType.Worker,130, 30, 8, 1, 5, 0.6f);
        _antStats[AntType.Warrior] = new AntStats(AntType.Warrior,180, 70, 2, 1, 4, 0.3f);
        _antStats[AntType.Scout] = new AntStats(AntType.Scout,80, 20, 2, 4, 7, 0.1f);

        _foodStats = new Dictionary<FoodType, FoodStats>();
        _foodStats[FoodType.Apple] = new FoodStats(FoodType.Apple, 10);
        _foodStats[FoodType.Bread] = new FoodStats(FoodType.Bread, 20);
        _foodStats[FoodType.Nectar] = new FoodStats(FoodType.Nectar, 60);

    }

    public static AntStats GetAntStatsByType(AntType type)
    {
        return _antStats[type];
    }

    public static FoodStats GetFoodStatsByType(int type)
    {
        return _foodStats[(FoodType)type];
    }

    public static HexCell CreateHexCellFromType(HexType type)
    {
        byte cost = 0;
        bool passable = true;

        switch (type)
        {
            case HexType.Base:
                cost = 1;
                passable = true;
                break;
            case HexType.Default:
                cost = 1;
                passable = true;
                break;
            case HexType.Dirt:
                cost = 2;
                passable = true;
                break;
            case HexType.Acid:
                cost = 10;
                passable = true;
                break;
            case HexType.Obstacle:
                cost = 255;
                passable = false;
                break;
            case HexType.EndOfMap:
                cost = 255;
                passable = false;
                break;
            case HexType.EnemyBase:
                cost = 255;
                passable = false;
                break;
            case HexType.Fake:
                cost = 1;
                passable = true;
                break;
        }

        return new HexCell(type, cost, passable);
    }
}

public class AntStats
{
    public AntType Type { get; }
    public int Health { get; }
    public int Damage { get; }
    public int Carrying { get; }
    public int Sight { get; }
    public int Speed { get; }
    public float SpawnRate { get; }

    public AntStats(AntType type,int health, int damage, int carrying, int sight, int speed, float spawnRate)
    {
        Type = type;
        Health = health;
        Damage = damage;
        Carrying = carrying;
        Sight = sight;
        Speed = speed;
        SpawnRate = spawnRate;
    }
}

public class FoodStats
{
    public FoodType Type { get; }
    public int Calories { get; }

    public FoodStats(FoodType type, int calories)
    {
        Type = type;
        Calories = calories;
    }
}

