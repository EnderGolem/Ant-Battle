using Server.Net.Models;

namespace Server.Combat;

public class Combat
{
    private CombatField _combatField = new CombatField();

    private List<HexCellHash> _homeCells = new List<HexCellHash>();

    private HashSet<HexCellHash> _hexVisibleThisTick = new HashSet<HexCellHash>();

    private Dictionary<string, Ant> _scouts = new Dictionary<string, Ant>();
    private Dictionary<string, Ant> _workers = new Dictionary<string, Ant>();

    private Dictionary<string, Ant> _unassignedAnts = new Dictionary<string, Ant>();

    private Dictionary<AntType, HashSet<HexCellHash>> _cellsOccupiedByAnts =
        new Dictionary<AntType, HashSet<HexCellHash>>(); 

    private GameState _currentGameState;
    public CombatField MemorizedFields => _combatField;

    public List<HexCellHash> HomeCells => _homeCells;

    public HashSet<HexCellHash> HexVisibleThisTick => _hexVisibleThisTick;

    public GameState CurrentGameState => _currentGameState;

    public Dictionary<string, Ant> Scouts => _scouts;
    public Dictionary<string, Ant> Workers => _workers;

    public Dictionary<string, Ant> UnassignedAnts => _unassignedAnts;

    public Dictionary<AntType, HashSet<HexCellHash>> CellsOccupiedByAnts => _cellsOccupiedByAnts;


    private Strategizer _strategizer;

    private ScoutingLogic _scoutingLogic;
    private WorkerLogic _workerLogic;

    public MovesRequest Tick(GameState gameState)
    {
        if (_homeCells.Count == 0)
        {
            foreach (var coord in gameState.Home)
            {
                _homeCells.Add(HexCellHash.FromCoordinate(coord));
            }

            _strategizer = new Strategizer(this);
            _scoutingLogic = new ScoutingLogic(this);
            _workerLogic = new WorkerLogic(this);

            foreach (var homeCell in _homeCells)
            {
                foreach (var hex in HexGridHelper.GetAllCellsInRadius(homeCell,6))
                {
                    _combatField.AddFakeCell(hex, Encyclopedia.CreateHexCellFromType(HexType.Fake));
                }
            }
        }


        _currentGameState = gameState;
        var input = new MovesRequest();
        _hexVisibleThisTick.Clear();
        _cellsOccupiedByAnts.Clear();


        CheckingMap(gameState);

        CheckingAnts(gameState);

        //CheckingFood(gameState);


        _strategizer.Strategize();
        _workerLogic.AssignWorkerTasks();

        //_strategizer.PostStrategize();
        _scoutingLogic.AssignScoutPoints();


        List<Move> moves = new List<Move>();
        moves.AddRange(_scoutingLogic.Scout());
        moves.AddRange(_workerLogic.GetWorkerMoves());



        input.Moves = moves;


        return input;
    }

    private void CheckingFood(GameState gameState)
    {
        var mapHashes = new HashSet<HexCellHash>(
            gameState.Map.Select(c => HexCellHash.FromCoordinate(new Coordinate(c.Q, c.R))));
        var foodHashes = new HashSet<HexCellHash>();

        // Добавляем/обновляем еду + наполняем foodHashes
        foreach (var f in gameState.Food)
        {
            var hash = HexCellHash.FromCoordinate(new Coordinate(f.Q, f.R));
            _combatField.AddFood(hash, f);
            foodHashes.Add(hash);
        }

        // Удаляем устаревшие записи
        var keysToRemove = _combatField.Field.Keys
            .Where(k => mapHashes.Contains(k) && !foodHashes.Contains(k))
            .ToList();

        foreach (var key in keysToRemove)
            _combatField.Remove(key);
    }


    private void CheckingAnts(GameState gameState)
    {
        foreach (var ant in gameState.Ants)
        {
            var stats = Encyclopedia.GetAntStatsByType(ant.Type);
            var pos = HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R});
            
            var cellsToFake = HexGridHelper.GetAllCellsInRadius(pos, stats.Speed + 3);

            foreach (var hash in cellsToFake)
            {
                _combatField.AddFakeCell(hash, Encyclopedia.CreateHexCellFromType(HexType.Fake));
            }

            var hexShouldBeVisible =
                HexGridHelper.GetAllCellsInRadius(HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R}), stats.Sight);

            bool endOfMapIsClose = false;
            foreach (var hex in hexShouldBeVisible)
            {
                if (!_hexVisibleThisTick.Contains(hex))
                {
                    endOfMapIsClose = true;
                    _combatField.Field[hex].SetType(HexType.EndOfMap);
                }
            }

           

            if (!_scouts.ContainsKey(ant.Id) && !_workers.ContainsKey(ant.Id))
            {
                _unassignedAnts.Add(ant.Id, ant);
            }

            if (_cellsOccupiedByAnts.TryGetValue(stats.Type, out var set))
            {
                set.Add(pos);
            }
            else
            {
                HashSet<HexCellHash> newSet = new HashSet<HexCellHash>();
                newSet.Add(pos);
                _cellsOccupiedByAnts[stats.Type] = newSet;
            }
        }
    }

    private void CheckingMap(GameState gameState)
    {
        foreach (var tile in gameState.Map)
        {
            var hash = HexCellHash.FromCoordinate(new Coordinate() { Q = tile.Q, R = tile.R });
            var hexCell = Encyclopedia.CreateHexCellFromType((HexType)tile.Type);
            if (hexCell.Type == HexType.Base)
            {
                if (_homeCells.Contains(hash))
                {
                    hexCell.SetType(HexType.EnemyBase);
                }
            }

            _hexVisibleThisTick.Add(hash);
            _combatField.SetHexCell(hash, hexCell);
        }
    }

    /// <summary>
    /// Принудительно перемещает всех ботов на 2 шага в указанном направлении
    /// </summary>
    /// <param name="moveRight">true для движения вправо, false для движения влево</param>
    /// <returns>MovesRequest с принудительными перемещениями</returns>
    public MovesRequest ForceMoveBots(bool moveRight)
    {
        var moves = new List<Move>();
        var allAnts = new Dictionary<string, Ant>();
        
        // Собираем всех муравьев
        foreach (var scout in _scouts)
            allAnts[scout.Key] = scout.Value;
        foreach (var worker in _workers)
            allAnts[worker.Key] = worker.Value;
        foreach (var unassigned in _unassignedAnts)
            allAnts[unassigned.Key] = unassigned.Value;

        foreach (var ant in allAnts)
        {
            var currentPos = new Coordinate() { Q = ant.Value.Q, R = ant.Value.R };
            var path = new List<Coordinate>();

            // Создаем путь из 2 шагов
            for (int i = 0; i < 2; i++)
            {
                if (moveRight)
                {
                    // Движение вправо в гексагональной сетке
                    currentPos = new Coordinate() { Q = currentPos.Q + 1, R = currentPos.R };
                }
                else
                {
                    // Движение влево в гексагональной сетке
                    currentPos = new Coordinate() { Q = currentPos.Q - 1, R = currentPos.R };
                }
                path.Add(new Coordinate() { Q = currentPos.Q, R = currentPos.R });
            }

            moves.Add(new Move()
            {
                Ant = ant.Value.Id,
                Path = path
            });
        }

        Console.WriteLine($"Принудительное перемещение {moves.Count} ботов {(moveRight ? "вправо" : "влево")}");
        
        return new MovesRequest() { Moves = moves };
    }

    /// <summary>
    /// Принудительно перемещает всех ботов в указанном направлении на заданное количество шагов
    /// </summary>
    /// <param name="direction">Направление движения (r, l, ru, rd, lu, ld)</param>
    /// <param name="steps">Количество шагов (1-10)</param>
    /// <returns>MovesRequest с принудительными перемещениями</returns>
    public MovesRequest ForceMoveBotsDirection(string direction, int steps = 2)
    {
        // Проверка корректности количества шагов
        if (steps < 1 || steps > 10)
        {
            Console.WriteLine($"Ошибка: количество шагов должно быть от 1 до 10, получено {steps}");
            steps = Math.Clamp(steps, 1, 10);
        }

        var moves = new List<Move>();
        var allAnts = new Dictionary<string, Ant>();

        // Собираем всех муравьев
        foreach (var scout in _scouts)
            allAnts[scout.Key] = scout.Value;
        foreach (var worker in _workers)
            allAnts[worker.Key] = worker.Value;
        foreach (var unassigned in _unassignedAnts)
            allAnts[unassigned.Key] = unassigned.Value;

        foreach (var ant in allAnts)
        {
            var currentPos = new Coordinate() { Q = ant.Value.Q, R = ant.Value.R };
            var path = new List<Coordinate>();

            // Создаем путь из указанного количества шагов
            for (int i = 0; i < steps; i++)
            {
                switch (direction.ToLower())
                {
                    case "r":  // вправо
                        currentPos = new Coordinate() { Q = currentPos.Q + 1, R = currentPos.R };
                        break;
                    case "l":  // влево
                        currentPos = new Coordinate() { Q = currentPos.Q - 1, R = currentPos.R };
                        break;
                    case "ru": // вправо-вверх (odd-r)
                        currentPos = new Coordinate() { Q = currentPos.Q + (currentPos.R % 2 == 0 ? 0 : 1), R = currentPos.R - 1 };
                        break;
                    case "rd": // вправо-вниз (odd-r)
                        currentPos = new Coordinate() { Q = currentPos.Q + (currentPos.R % 2 == 0 ? 0 : 1), R = currentPos.R + 1 };
                        break;
                    case "lu": // влево-вверх (odd-r)
                        currentPos = new Coordinate() { Q = currentPos.Q - (currentPos.R % 2 == 1 ? 0 : 1), R = currentPos.R - 1 };
                        break;
                    case "ld": // влево-вниз (odd-r)
                        currentPos = new Coordinate() { Q = currentPos.Q - (currentPos.R % 2 == 1 ? 0 : 1), R = currentPos.R + 1 };
                        break;
                    default:
                        Console.WriteLine($"Ошибка: неизвестное направление '{direction}'. Допустимые значения: r, l, ru, rd, lu, ld");
                        return new MovesRequest() { Moves = new List<Move>() }; // Возвращаем пустой список перемещений
                }
                path.Add(new Coordinate() { Q = currentPos.Q, R = currentPos.R });
            }

            moves.Add(new Move()
            {
                Ant = ant.Value.Id,
                Path = path
            });
        }

        Console.WriteLine($"Принудительное перемещение {moves.Count} ботов в направлении {direction} на {steps} шагов");

        return new MovesRequest() { Moves = moves };
    }
}

