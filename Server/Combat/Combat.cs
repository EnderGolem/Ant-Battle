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
        }


        _currentGameState = gameState;
        var input = new MovesRequest();
        _hexVisibleThisTick.Clear();
        _cellsOccupiedByAnts.Clear();


        CheckingMap(gameState);

        CheckingAnts(gameState);



        _strategizer.Strategize();
        _workerLogic.AssignWorkerTasks();

        _strategizer.PostStrategize();
        _scoutingLogic.AssignScoutPoints();


        List<Move> moves = new List<Move>();
        moves.AddRange(_scoutingLogic.Scout());
        moves.AddRange(_workerLogic.GetWorkerMoves());

        //List<Move> moves = new List<Move>();
        //for (int i = 0; i < gameState.Ants.Count; i++)
        //{
        //    Move move = new Move();
        //    move.Ant = gameState.Ants[i].Id;
        //    var antPos = new HexCellHash(gameState.Ants[i].Q, gameState.Ants[i].R);
        //    List<Coordinate> path = new List<Coordinate>(3);


        //    for (int j = 0; j < 3; j++)
        //    {
        //        if (AntType.Scout == gameState.Ants[i].Type)
        //            path.Add((antPos + HexCellHash.RightUp() * (j + 1)).ToCoordinate());
        //        if (AntType.Worker == gameState.Ants[i].Type)
        //            path.Add((antPos + HexCellHash.Left() * (j + 1)).ToCoordinate());
        //        if (AntType.Warrior == gameState.Ants[i].Type)
        //            path.Add((antPos + HexCellHash.LeftDown() * (j + 1)).ToCoordinate());
        //    }
        //    move.Path = path;
        //    moves.Add(move);
        //}


        input.Moves = moves;


        return input;
    }

    private void CheckingAnts(GameState gameState)
    {
        foreach (var ant in gameState.Ants)
        {
            var stats = Encyclopedia.GetAntStatsByType(ant.Type);
            var pos = HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R});

            var cellsToFake = HexGridHelper.GetAllCellsInRadius(pos, stats.Speed);

            foreach (var hash in cellsToFake)
            {
                _combatField.AddFakeCell(hash, Encyclopedia.CreateHexCellFromType(HexType.Fake));
            }

            var hexShouldBeVisible =
                HexGridHelper.GetAllCellsInRadius(HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R}), stats.Sight);

            foreach (var hex in hexShouldBeVisible)
            {
                if (!_hexVisibleThisTick.Contains(hex))
                {
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
}

