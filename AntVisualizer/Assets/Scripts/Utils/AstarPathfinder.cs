using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

    public class AstarPathfinder
    {

        private FastPriorityQueue<HexNode> openSet;
        private Dictionary<HexCellHash, HexNode> visited;

        private List<HexNeighborInfo> neighbors = new List<HexNeighborInfo>();

        private delegate int HeuristicFunction(HexCellHash startNode, HexCellHash targetNode);

        private HeuristicFunction heuristic;

        private HexGridHelper _helper;
        
        private IReadOnlyDictionary<HexCellHash, HexCell> _field;

        private int _pathfindingActorId;

        public AstarPathfinder(int graphSize, HexGridHelper helper)
        {
            _helper = helper;
            Allocate(graphSize);
        }

        public void Allocate(int graphSize)
        {
            openSet = new FastPriorityQueue<HexNode>(graphSize / 2 + 10);
            visited = new Dictionary<HexCellHash, HexNode>(graphSize / 2 + 10);
        }

        public List<HexCellHash> Pathfind(IReadOnlyDictionary<HexCellHash, HexCell> field, HexCellHash startPos,
            HexCellHash targetPos, int pathfindForActorId = -1)
        {
            if (!field.ContainsKey(targetPos))
            {
                return null;
            }


            _field = field;
            _pathfindingActorId = pathfindForActorId;
            openSet.Clear();
            visited.Clear();

            heuristic = _helper.ManhattanDistance;

            var startNode = new HexNode { pos = startPos, cost = 0, prev = null };
            openSet.Enqueue(startNode, 0);
            visited[startPos] = startNode;

            while (openSet.Count > 0)
            {
                var cur = openSet.Dequeue();
                //Debug.Log(cur.position);
                if (cur.pos == targetPos)
                {
                    var resList = new List<HexCellHash>();
                    while (true)
                    {
                        resList.Add(cur.pos);
                        if (cur.prev == null)
                        {
                            return resList;
                        }
                        else
                        {
                            cur = cur.prev;
                        }
                    }
                }

                var neighbors = GetNeighbors(cur.pos);
                foreach (var neighbor in neighbors)
                {
                    if (visited.TryGetValue(neighbor.neighbor, out HexNode n))
                    {
                        if (n.cost > cur.cost + neighbor.cost)
                        {
                            n.cost = cur.cost + neighbor.cost;
                            n.prev = cur;
                            var f = n.cost + heuristic(n.pos, targetPos);
                            openSet.UpdatePriority(n, f);
                        }
                    }
                    else
                    {
                        var node = new HexNode()
                            { pos = neighbor.neighbor, prev = cur, cost = (cur.cost + neighbor.cost) };
                        visited[neighbor.neighbor] = node;
                        openSet.Enqueue(node, cur.cost + neighbor.cost + heuristic(neighbor.neighbor, targetPos));
                    }
                }
            }

            return null;
        }

        List<HexNeighborInfo> GetNeighbors(HexCellHash pos)
        {
            neighbors.Clear();

            foreach (var neighborPos in _helper.GetPrimaryNeighbors(pos))
            {
                if (_field.TryGetValue(neighborPos, out var hexCell))
                {
                    if (/*!hexCell.IsOccupied &&*/ !hexCell.IsOccupied)
                    {
                        neighbors.Add(new HexNeighborInfo
                        {
                            neighbor = neighborPos,
                            cost = 1
                        });
                    }
                }
            }
            
            return neighbors;
        }


    }

    public class HexNode : FastPriorityQueueNode
    {
        public HexCellHash pos;
        public float cost;
        public HexNode prev;
    }

    public struct HexNeighborInfo
    {
        public HexCellHash neighbor;
        public float cost;
    }

