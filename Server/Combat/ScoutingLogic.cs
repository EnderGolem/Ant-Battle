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
                    var cost = EstimateCostForPoint(hex.Key, hex.Value, 
                        HexCellHash.FromCoordinate(new Coordinate{Q = scout.Value.Q, R = scout.Value.R}),scout.Value.Type);
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

            if (_combat.CellsOccupiedByAnts.TryGetValue(ant.Type, out var set))
            {
                set.Remove(HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R}));
            }

            var calculatedPath = _pathfinder.Pathfind(
                _combat.MemorizedFields.Field, _combat.CellsOccupiedByAnts,ant.Type,HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R}), command.Value);

            if (calculatedPath != null && calculatedPath.Count > 0)
            {
                Move move = new Move();
                var length = Math.Min(calculatedPath.Count-1, Encyclopedia.GetAntStatsByType(ant.Type).Speed);
                List<Coordinate> list = new List<Coordinate>();

                for (int i = 0; i < length; i++)
                {
                    list.Add(calculatedPath[calculatedPath.Count - 2 - i].ToCoordinate());
                }

                move.Path = list;
                move.Ant = ant.Id;
                if (_combat.CellsOccupiedByAnts.TryGetValue(ant.Type, out var st))
                {
                    st.Add(calculatedPath[calculatedPath.Count - 2 - (length -1)]);
                }
                
                res.Add(move);
            }

        }

        return res;
    }

    public float EstimateCostForPoint(HexCellHash point, HexCell cell, HexCellHash currentPosition, AntType antType)
    {
        if (cell.Type == HexType.EndOfMap)
        {
            return -100000;
        }

        if (cell.Type != HexType.Fake)
        {
            return -1000;
        }

        var path = _pathfinder.Pathfind(_combat.MemorizedFields.Field, _combat.CellsOccupiedByAnts, antType,
            currentPosition,
            point);

        if (path == null || path.Count < 2)
        {
            return -1000;
        }

        float cost = 0;

        var homeDistCoef = 1;
        if (antType == AntType.Worker)
        {
            homeDistCoef = 3;
        }

        cost -= HexGridHelper.ManhattanDistance(_combat.HomeCells[0], point) * homeDistCoef;
        cost -= HexGridHelper.ManhattanDistance(currentPosition, point) * 2;

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

        int closeUndiscoveredPoints = 0;
        foreach (var nearPoint in HexGridHelper.GetAllCellsInRadius(point,3))
        {
            if (_combat.MemorizedFields.Field.TryGetValue(nearPoint, out var c))
            {
                if (c.Type == HexType.Fake)
                {
                    closeUndiscoveredPoints += 1;
                }
            }
            else
            {
                closeUndiscoveredPoints += closeUndiscoveredPoints;
            }
        }

        cost += closeUndiscoveredPoints;

        return cost;
    }
}