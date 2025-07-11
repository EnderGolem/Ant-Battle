// AntBotLogic.cs
// Генератор путей и логики передвижения для муравьёв в DatsPulse
// Автор: ChatGPT, 2025-07-11
// Предназначен для интеграции в существующий клиент. Принимает GameState и формирует MovesRequest.

using System;
using System.Collections.Generic;
using System.Linq;
using Server.Net.Models;

namespace Bot.Logic;

/// <summary>
/// Главный класс-стратег, формирующий команды перемещения для всех видимых юнитов.
/// </summary>
public class AntBotLogic
{
    // — направления в axial‑координатах (q, r)
    private static readonly (int dq, int dr)[] Directions =
    {
        (1, 0),  // восток
        (1, -1), // северо‑восток
        (0, -1), // северо‑запад
        (-1, 0), // запад
        (-1, 1), // юго‑запад
        (0, 1)   // юго‑восток
    };

    /// <summary>
    /// Формирует MovesRequest на основе текущего GameState.
    /// </summary>
    public MovesRequest BuildMoves(GameState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        var request = new MovesRequest();

        // Сохраняем занятые гексы, чтобы минимизировать столкновения.
        var occupied = new HashSet<(int q, int r)>(state.Ants.Select(a => (a.Q, a.R)));

        foreach (var ant in state.Ants)
        {
            // Получаем max очков передвижения для данного типа.
            var speed = GetMovementPoints((int)ant.Type);
            if (speed <= 0) continue;

            Coordinate? target = null;

            if (ant.Food?.Amount > 0)
            {
                // Несёт ресурсы — возвращаемся на ближайший гекс муравейника.
                target = state.Home.OrderBy(h => HexDistance(ant.Q, ant.R, h.Q, h.R)).FirstOrDefault();
            }
            else
            {
                // Ищем ближайший ресурс.
                target = state.Food
                    .Where(f => f.Amount > 0)
                    .OrderBy(f => HexDistance(ant.Q, ant.R, f.Q, f.R))
                    .Select(f => new Coordinate { Q = f.Q, R = f.R })
                    .FirstOrDefault();
            }

            if (target == null) continue;

            var path = FindPath(ant, target, state, speed);

            if (path.Count > 0)
            {
                occupied.Add((path.Last().Q, path.Last().R));

                request.Moves.Add(new Move
                {
                    Ant = ant.Id,
                    Path = path
                });
            }
        }

        return request;
    }

    #region Path‑finding
    //TODO Переписать тут ошибки
    private static IList<Coordinate> FindPath(Ant ant, Coordinate target, GameState state, int maxSteps)
    {
        var start = new Coordinate { Q = ant.Q, R = ant.R };
        var frontier = new Queue<Coordinate>();
        frontier.Enqueue(start);

        var cameFrom = new Dictionary<(int q, int r), (int q, int r)?>
        {
            [(start.Q, start.R)] = null
        };

        var costSoFar = new Dictionary<(int q, int r), int>
        {
            [(start.Q, start.R)] = 0
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.Q == target.Q && current.R == target.R) break;

            foreach (var next in GetNeighbors(current, state))
            {
                var nextKey = (next.Q, next.R);
                var tile = state.Map.FirstOrDefault(t => t.Q == next.Q && t.R == next.R);
                var moveCost = tile?.Cost ?? 1;
                var newCost = costSoFar[(current.Q, current.R)] + moveCost;

                if (moveCost == int.MaxValue) continue;

                if (newCost > maxSteps) continue;

                if (!costSoFar.ContainsKey(nextKey) || newCost < costSoFar[nextKey])
                {
                    costSoFar[nextKey] = newCost;
                    cameFrom[nextKey] = (current.Q, current.R);
                    frontier.Enqueue(next);
                }
            }
        }

        // Если цели нет в cameFrom — путь не найден в рамках maxSteps.
        if (!cameFrom.ContainsKey((target.Q, target.R))) return new List<Coordinate>();

        // Восстанавливаем путь от цели к старту.
        var reversed = new List<Coordinate>();
        var step = (target.Q, target.R);
        while (cameFrom[step] != null)
        {
            reversed.Add(new Coordinate { Q = step.Q, R = step.R });
            step = cameFrom[step].Value;
        }

        reversed.Reverse();
        return reversed;
    }

    private static IEnumerable<Coordinate> GetNeighbors(Coordinate c, GameState state)
    {
        foreach (var (dq, dr) in Directions)
        {
            var nq = c.Q + dq;
            var nr = c.R + dr;

            var tile = state.Map.FirstOrDefault(t => t.Q == nq && t.R == nr);
            if (tile == null) continue;
            if ((int)tile.Type == 5) continue;        

            yield return new Coordinate { Q = nq, R = nr };
        }
    }

    #endregion

    #region Helpers

    private static int HexDistance(int q1, int r1, int q2, int r2)
    {
        return (Math.Abs(q1 - q2) + Math.Abs(r1 - r2) + Math.Abs((q1 + r1) - (q2 + r2))) / 2;
    }

    private static int GetMovementPoints(int antType) => antType switch
    {
        2 => 7, // Разведчик
        1 => 4, // Боец
        0 => 5, // Рабочий
        _ => 4
    };

    #endregion
}
