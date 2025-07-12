using Server.Net.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
                float maxCost = float.MinValue;
                HexCellHash bestPos = new HexCellHash();
                foreach (var hex in _combat.MemorizedFields.Field)
                {
                    // Пропускаем точки, которые уже назначены другим разведчикам
                    if (_pointsToScout.Contains(hex.Key))
                        continue;
                        
                    var cost = EstimateCostForPoint(hex.Key, hex.Value, 
                        HexCellHash.FromCoordinate(new Coordinate{Q = scout.Value.Q, R = scout.Value.R}), scout.Value.Type);
                    if (cost > maxCost)
                    {
                        maxCost = cost;
                        bestPos = hex.Key;
                    }
                }

                if (maxCost > float.MinValue)
                {
                    _pointsToScout.Add(bestPos);
                    _scoutCommands.Add(scout.Key, bestPos);
                }
            }
        }
        
        
    }

    public List<Move> Scout()
    {
        ConcurrentBag<Move> res = new ConcurrentBag<Move>();

        Parallel.ForEach(_scoutCommands, command =>
        {
            var ant = _combat.Scouts[command.Key];

            var localPathfinder = new AstarPathfinder(10000);
            var calculatedPath = localPathfinder.Pathfind(
                _combat.MemorizedFields.Field,
                _combat.CellsOccupiedByAnts,
                ant.Type,
                HexCellHash.FromCoordinate(new Coordinate { Q = ant.Q, R = ant.R }),
                command.Value);

            if (calculatedPath != null && calculatedPath.Count > 0)
            {
                Move move = new Move();
                var length = Math.Min(calculatedPath.Count - 1, Encyclopedia.GetAntStatsByType(ant.Type).Speed);
                List<Coordinate> list = new List<Coordinate>();

                for (int i = 0; i < length; i++)
                {
                    list.Add(calculatedPath[calculatedPath.Count - 2 - i].ToCoordinate());
                }

                move.Path = list;
                move.Ant = ant.Id;
                res.Add(move);
            }

        });

        return res.ToList();
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

        // Базовый приоритет для неизученных клеток
        cost += 1000;

        // Штраф за расстояние от дома - чем дальше, тем хуже
        var homeDistCoef = 3;
        if (antType == AntType.Worker)
        {
            homeDistCoef = 8; // Рабочие должны держаться ближе к дому
        }
        cost -= HexGridHelper.ManhattanDistance(_combat.HomeCells[0], point) * homeDistCoef;

        // Штраф за расстояние от текущей позиции - предпочитаем ближние цели
        cost -= HexGridHelper.ManhattanDistance(currentPosition, point) * 5;

        // Бонус за распределение разведчиков - поощряем разнообразие целей
        int minDistToOtherPoints = int.MaxValue;
        if (_pointsToScout.Count > 0)
        {
            foreach (var hash in _pointsToScout)
            {
                var dist = HexGridHelper.ManhattanDistance(hash, point);
                if (dist < minDistToOtherPoints)
                {
                    minDistToOtherPoints = dist;
                }
            }
            // Поощряем точки, которые далеко от уже назначенных целей
            cost += minDistToOtherPoints * 2;
        }

        // Бонус за количество неизученных клеток рядом
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
                closeUndiscoveredPoints += 1;
            }
        }

        cost += closeUndiscoveredPoints * 3; // Увеличили коэффициент для приоритета областей с много неизученного

        return cost;
    }
}