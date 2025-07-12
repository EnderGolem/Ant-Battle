using Server.Net.Models;

namespace Server.Combat;

public class Combat
{
    private CombatField _combatField = new CombatField();

    private List<HexCellHash> _homeCells = new List<HexCellHash>();

    private HashSet<HexCellHash> _hexVisibleThisTick = new HashSet<HexCellHash>();

    private Dictionary<string, Ant> _scouts = new Dictionary<string, Ant>();

    private Dictionary<string, Ant> _unassignedAnts = new Dictionary<string, Ant>();

    private GameState _currentGameState;
    public CombatField CombatField => _combatField;

    public List<HexCellHash> HomeCells => _homeCells;

    public HashSet<HexCellHash> HexVisibleThisTick => _hexVisibleThisTick;

    public GameState CurrentGameState => _currentGameState;

    public Dictionary<string, Ant> Scouts => _scouts;

    public Dictionary<string, Ant> UnassignedAnts => _unassignedAnts;


    private Strategizer _strategizer;

    private ScoutingLogic _scoutingLogic;

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
        }


        _currentGameState = gameState;
        var input = new MovesRequest();
        List<Move> moves = new List<Move>();
        _hexVisibleThisTick.Clear();
        foreach (var tile in gameState.Map)
        {
            var hash = new HexCellHash(tile.Q, tile.R);
            var hexCell = Encyclopedia.CreateHexCellFromType((HexType)tile.Type);
            if (hexCell.Type == HexType.Base)
            {
                if(_homeCells.Contains(hash))
                {
                    hexCell.SetType(HexType.EnemyBase);
                }
            }

            _hexVisibleThisTick.Add(hash);
            _combatField.SetHexCell(hash, hexCell);
        }

        foreach (var ant in gameState.Ants)
        {
            var stats = Encyclopedia.GetAntStatsByType(ant.Type);
            
            var cellsToFake = HexGridHelper.GetAllCellsInRadius(new HexCellHash(ant.Q, ant.R), stats.Speed);

            foreach (var hash in cellsToFake)
            {
                _combatField.AddFakeCell(hash,Encyclopedia.CreateHexCellFromType(HexType.Fake));
            }

            var hexShouldBeVisible = 
                HexGridHelper.GetAllCellsInRadius(new HexCellHash(ant.Q, ant.R), stats.Sight);

            foreach (var hex in hexShouldBeVisible)
            {
                if (!_hexVisibleThisTick.Contains(hex))
                {
                    _combatField.Field[hex].SetType(HexType.EndOfMap);
                }
            }

            if (!_scouts.ContainsKey(ant.Id))
            {
                _unassignedAnts.Add(ant.Id, ant);
            }
        }
        
        _strategizer.Strategize();
        
        _scoutingLogic.AssignScoutPoints();
        moves.AddRange(_scoutingLogic.Scout());
        
        
        /*List<Move> moves = new List<Move>();
        for (int i = 0; i < gameState.Ants.Count; i++)
        {
            Move move = new Move();
            move.Ant = gameState.Ants[i].Id;
            var antPos = new HexCellHash(gameState.Ants[i].Q, gameState.Ants[i].R);
            List<Coordinate> path = new List<Coordinate>(3);
            for (int j = 0; j < 3; j++)
            {
                path.Add((antPos + HexCellHash.Left() * j).ToCoordinate());
            }

            moves.Add(move);
        }*/

        input.Moves = moves;


        return input;
    }
    
}

