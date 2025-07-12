using Server.Net.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Server.Combat;

public class WorkerLogic
{
    private Combat _combat;
    private Dictionary<string, WorkerState> _workerStates = new Dictionary<string, WorkerState>();
    private Dictionary<string, HexCellHash> _workerTargets = new Dictionary<string, HexCellHash>();
    private AstarPathfinder _pathfinder;

    public WorkerLogic(Combat combat)
    {
        _combat = combat;
        _pathfinder = new AstarPathfinder(10000);
    }

    public void AssignWorkerTasks()
    {
        // Проверяем состояние всех рабочих
        foreach (var worker in _combat.Workers)
        {
            if (!_workerStates.ContainsKey(worker.Key))
            {
                _workerStates[worker.Key] = WorkerState.SearchingFood;
            }

            var ant = worker.Value;
            var currentPos = HexCellHash.FromCoordinate(new Coordinate(){Q = ant.Q, R = ant.R});
            
            // Обновляем состояние в зависимости от текущей ситуации
            UpdateWorkerState(worker.Key, ant, currentPos);
            

            if(_workerStates[worker.Key] == WorkerState.SearchingFood)
            {
                _combat.UnassignedAnts.Add(worker.Key, worker.Value);
            }

        }
    }

    private void UpdateWorkerState(string workerId, Ant ant, HexCellHash currentPos)
    {
        var state = _workerStates[workerId];
        
        switch (state)
        {
            case WorkerState.SearchingFood:
                // Если муравей несет еду, переходим к возвращению домой
                if (ant.Food.Amount > 0)
                {
                    _workerStates[workerId] = WorkerState.ReturningHome;
                    _workerTargets[workerId] = _combat.HomeCells[0]; // Идем к базе
                }
                else
                {
                    // Ищем ближайшую еду
                    FindNearestFood(workerId, currentPos);
                }
                break;
                
            case WorkerState.MovingToFood:
                // Проверяем, достигли ли мы цели
                if (_workerTargets.ContainsKey(workerId) && currentPos == _workerTargets[workerId])
                {
                    _workerStates[workerId] = WorkerState.CollectingFood;
                }
                // Проверяем, все ли еще есть еда на целевой позиции
                else if (_workerTargets.ContainsKey(workerId))
                {
                    var targetPos = _workerTargets[workerId];
                    bool foodStillExists = _combat.CurrentGameState.Food.Any(f => 
                        HexCellHash.FromCoordinate(new Coordinate(){Q = f.Q, R = f.R}) == targetPos);
                    
                    if (!foodStillExists)
                    {
                        // Еда исчезла, ищем новую
                        _workerStates[workerId] = WorkerState.SearchingFood;
                        _workerTargets.Remove(workerId);
                    }
                }
                break;
                
            case WorkerState.CollectingFood:
                // После сбора еды возвращаемся домой
                if (ant.Food.Amount > 0)
                {
                    _workerStates[workerId] = WorkerState.ReturningHome;
                    _workerTargets[workerId] = _combat.HomeCells[0];
                }
                break;
                
            case WorkerState.ReturningHome:
                // Если достигли базы, начинаем поиск новой еды
                if (_combat.HomeCells.Contains(currentPos))
                {
                    _workerStates[workerId] = WorkerState.SearchingFood;
                    _workerTargets.Remove(workerId);
                }
                break;
        }
    }

    private void FindNearestFood(string workerId, HexCellHash currentPos)
    {
        FoodOnMap? nearestFood = null;
        float minDistance = float.MaxValue;

        foreach (var food in _combat.CurrentGameState.Food) //Если не сработает, то верни
        //foreach (var foodKeyValue in _combat.MemorizedFields.Foods)
        {
            //var food = foodKeyValue.Value;
            var foodPos = HexCellHash.FromCoordinate(new Coordinate(){Q = food.Q, R = food.R});
            float distance = HexGridHelper.ManhattanDistance(currentPos, foodPos);
            
            // Проверяем, не занята ли уже эта еда другим рабочим
            bool isTargeted = _workerTargets.Values.Contains(foodPos);
            
            if (distance < minDistance && !isTargeted && (!_combat.HomeCells.Contains(foodPos)) )
            {
                minDistance = distance;
                nearestFood = food;
            }
        }

        if (nearestFood != null)
        {
            _workerStates[workerId] = WorkerState.MovingToFood;
            _workerTargets[workerId] = HexCellHash.FromCoordinate(new Coordinate(){Q = nearestFood.Q, R = nearestFood.R});
        }

        //ЗАГЛУШКА возможно стоит заменить
        if (nearestFood == null)
        {
            _workerStates[workerId] = WorkerState.SearchingFood;
            _workerTargets[workerId] = currentPos + HexCellHash.RightUp();
        }
    }

    public List<Move> GetWorkerMoves()
    {
        ConcurrentBag<Move> moves = new ConcurrentBag<Move>();

        Parallel.ForEach(_combat.Workers, worker =>
        {
            var ant = worker.Value;
            var currentPos = HexCellHash.FromCoordinate(new Coordinate(ant.Q, ant.R));
            var state = _workerStates[worker.Key];

            if (state == WorkerState.CollectingFood)
            {
                moves.Add(new Move
                {
                    Ant = ant.Id,
                    Path = new List<Coordinate>()
                });
                return;
            }

            if (_workerTargets.ContainsKey(worker.Key))
            {
                var targetPos = _workerTargets[worker.Key];
                var localPathfinder = new AstarPathfinder(10000);
                var calculatedPath = localPathfinder.Pathfind(
                    _combat.MemorizedFields.Field,
                    _combat.CellsOccupiedByAnts,
                    ant.Type,
                    currentPos,
                    targetPos);

                if (calculatedPath != null && calculatedPath.Count > 1)
                {
                    Move move = new Move();
                    var antStats = Encyclopedia.GetAntStatsByType(ant.Type);
                    var maxSteps = Math.Min(calculatedPath.Count - 1, antStats.Speed);
                    List<Coordinate> path = new List<Coordinate>();

                    for (int i = 0; i < maxSteps; i++)
                    {
                        path.Add(calculatedPath[calculatedPath.Count - 2 - i].ToCoordinate());
                    }

                    move.Path = path;
                    move.Ant = ant.Id;
                    moves.Add(move);
                }
            }
        });

        return moves.ToList();
    }
}

public enum WorkerState
{
    SearchingFood,
    MovingToFood,
    CollectingFood,
    ReturningHome,
}
