using Server.Net.Models;

namespace Server.Combat;

public class ScoutingLogic
{
    private Combat _combat;

    private HashSet<HexCellHash> _pointsToScout = new HashSet<HexCellHash>();

    private Dictionary<string, HexCellHash> _scoutCommands = new Dictionary<string, HexCellHash>();

    private AstarPathfinder _pathfinder;

    public ScoutingLogic(Combat combat)
    {
        _combat = combat;
        _pathfinder = new AstarPathfinder(10000);
    }

    public void AssignScoutPoints()
    {
        List<string> _commandsToRemove = new List<string>();
        foreach (var command in _scoutCommands)
        {
            if (_combat.HexVisibleThisTick.Contains(command.Value))
            {
                _pointsToScout.Remove(command.Value);
                _commandsToRemove.Add(command.Key);
            }
        }

        foreach (var cmd in _commandsToRemove)
        {
            _scoutCommands.Remove(cmd);
        }
        
        foreach (var scout in _combat.Scouts)
        {
            if (!_scoutCommands.ContainsKey(scout.Key))
            {
                float maxCost = int.MinValue;
                HexCellHash bestPos = new HexCellHash();
                foreach (var hex in _combat.MemorizedFields.Field)
                {
                    var cost = EstimateCostForPoint(hex.Key, hex.Value);
                    if (cost > maxCost)
                    {
                        maxCost = cost;
                        bestPos = hex.Key;
                    }
                }

                if (maxCost > int.MinValue)
                {
                    _pointsToScout.Add(bestPos);
                    _scoutCommands.Add(scout.Key, bestPos);
                }
            }
        }
        
        
    }

    public List<Move> Scout()
    {
        List<Move> res = new List<Move>();
        foreach (var command in _scoutCommands)
        {
            var ant = _combat.Scouts[command.Key];
            
            var calculatedPath = _pathfinder.Pathfind(_combat.MemorizedFields.Field, new HexCellHash(ant.Q, ant.R), command.Value);

            if (calculatedPath != null && calculatedPath.Count > 0)
            {
                Move move = new Move();
                var length = Math.Min(calculatedPath.Count, Encyclopedia.GetAntStatsByType(ant.Type).Speed);
                List<Coordinate> list = new List<Coordinate>();

                for (int i = 0; i < length; i++)
                {
                    list.Add(calculatedPath[calculatedPath.Count - 1 - i].ToCoordinate());
                }

                move.Path = list;
                move.Ant = ant.Id;
                res.Add(move);
            }

        }

        return res;
    }

    public float EstimateCostForPoint(HexCellHash point, HexCell cell)
    {
        if (cell.Type != HexType.Fake)
        {
            return -100;
        }

        float cost = 0;


        cost -= HexGridHelper.ManhattanDistance(_combat.HomeCells[0], point);

        int maxDistToOtherPoints = int.MinValue;

        foreach (var hash in _pointsToScout)
        {
            var dist = HexGridHelper.ManhattanDistance(hash, point);
            if (dist > maxDistToOtherPoints)
            {
                maxDistToOtherPoints = dist;
            }
        }

        cost += maxDistToOtherPoints;

        return cost;
    }
}