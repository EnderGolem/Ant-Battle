using Server.Combat;
using Server.Net.Models;

namespace ServerTest;

public class WorkerLogicTest
{
    [Test]
    public void TestWorkerLogicBasics()
    {
        // Создаем тестовое игровое состояние
        var gameState = new GameState
        {
            Ants = new List<Ant>
            {
                new Ant
                {
                    Id = "worker1",
                    Type = AntType.Worker,
                    Q = 0,
                    R = 0,
                    Health = 130,
                    Food = new AntFood { Amount = 0, Type = 0 }
                }
            },
            Food = new List<FoodOnMap>
            {
                new FoodOnMap
                {
                    Q = 2,
                    R = 2,
                    Amount = 10,
                    Type = FoodType.Apple
                }
            },
            Home = new List<Coordinate>
            {
                new Coordinate { Q = 0, R = 0 }
            },
            Map = new List<MapTile>
            {
                new MapTile { Q = 0, R = 0, Type = HexType.Base, Cost = 1 },
                new MapTile { Q = 1, R = 0, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 2, R = 0, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 0, R = 1, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 1, R = 1, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 2, R = 1, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 0, R = 2, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 1, R = 2, Type = HexType.Default, Cost = 1 },
                new MapTile { Q = 2, R = 2, Type = HexType.Default, Cost = 1 }
            }
        };

        // Создаем Combat и обрабатываем игровое состояние
        var combat = new Combat();
        var moves = combat.Tick(gameState);

        // Проверяем, что были созданы ходы
        Assert.IsNotNull(moves);
        Assert.IsNotNull(moves.Moves);
        
        // Рабочий должен начать движение к еде
        Assert.IsTrue(moves.Moves.Count > 0, "Worker should create moves");
        
        var workerMove = moves.Moves.FirstOrDefault(m => m.Ant == "worker1");
        Assert.IsNotNull(workerMove, "Worker should have a move");
        Assert.IsTrue(workerMove.Path.Count > 0, "Worker should have a path");
    }
}
