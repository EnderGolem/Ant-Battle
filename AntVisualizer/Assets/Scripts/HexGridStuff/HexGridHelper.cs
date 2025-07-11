using System.Collections;
using System.Collections.Generic;
using ElectricServiceCompany;
using Game.Scripts;
using UnityEngine;

public class HexGridHelper
{
   private AstarPathfinder _astarPathfinder;

   private HexCellHash[] _directions = new[]
   {
      new HexCellHash(1,0), new HexCellHash(1,-1), new HexCellHash(0,-1),
      new HexCellHash(-1,0), new HexCellHash(-1, 1), new HexCellHash(0, 1)
   };

   private HexCellHash[] _diagonals = new[]
   {
      new HexCellHash(2,-1), new HexCellHash(1,-2), new HexCellHash(-1,-1),
      new HexCellHash(-2,1), new HexCellHash(-1, 2), new HexCellHash(1, 1)
   };
   
   public int ManhattanDistance(HexCellHash cell1, HexCellHash cell2)
   {
      return (Mathf.Abs(cell1.q - cell2.q)
              + Mathf.Abs(cell1.q + cell1.r - cell2.q - cell2.r)
                 + Mathf.Abs(cell1.r - cell2.r)) / 2;
   }

   public HexCellHash[] GetPrimaryDirections()
   {
      return _directions;
   }

   public HexCellHash[] GetDiagonalDirections()
   {
      return _diagonals;
   }

   public HexCellHash[] GetPrimaryNeighbors(HexCellHash cell)
   {
      var neighbors = new HexCellHash[GetPrimaryDirections().Length];

      for (int i = 0; i < GetPrimaryDirections().Length; i++)
      {
         neighbors[i] = cell + GetPrimaryDirections()[i];
      }

      return neighbors;
   }

   public HexCellHash[] GetDiagonalNeighbors(HexCellHash cell)
   {
      var neighbors = new HexCellHash[GetDiagonalDirections().Length];

      for (int i = 0; i < GetDiagonalDirections().Length; i++)
      {
         neighbors[i] = cell + GetDiagonalDirections()[i];
      }

      return neighbors;
   }

   public List<HexCellHash> GetAllCellsInRadius(HexCellHash center, int radius)
   {
      
      List<HexCellHash> list = new List<HexCellHash>();

      for (int q = -radius; q <= radius; q++)
      {
         for (int r = Mathf.Max(-radius, -q-radius); r <= Mathf.Min(radius, -q + radius); r++)
         {
            list.Add(center + new HexCellHash(q,r));
         }
      }
      
      return list;
   }

   public HexCellHash RoundToHex(Vector3 position)
   {
      var q = Mathf.RoundToInt(position.x);
      var r = Mathf.RoundToInt(position.y);
      var s = Mathf.RoundToInt(position.z);

      var qDiff = Mathf.Abs(q - position.x);
      var rDiff = Mathf.Abs(r - position.y);
      var sDiff = Mathf.Abs(s - position.z);
      
      if (qDiff > rDiff && qDiff > sDiff)
      {
         q = -r - s;
      }
      else if (rDiff > sDiff)
      {
         r = -q - s;
      }

      return new HexCellHash(q, r);
   }

   public List<HexCellHash> CalculatePath(IReadOnlyDictionary<HexCellHash,
      HexCell> field, HexCellHash startPos, HexCellHash targetPos, 
      int actorIdForCalculation = -1)
   {
      if (_astarPathfinder == null)
      {
         _astarPathfinder = new AstarPathfinder(field.Count, this);
      }

      return _astarPathfinder.Pathfind(field, startPos, targetPos, actorIdForCalculation);
   }

   public List<HexCellHash> GetAllPossibleDestinationsForCell(IReadOnlyDictionary<HexCellHash, HexCell> field, 
      HexCellHash pos, int maxDistance,  int actorIdForCalculation = -1)
   {
      var list = GetAllCellsInRadius(pos, maxDistance);

      list.RemoveAll((cell) =>
      {
         var path = CalculatePath(field,  pos, cell, actorIdForCalculation);
         if (path == null || path.Count > maxDistance + 1)
         {
            return true;
         }

         return false;
      });

      return list;
   }

   /*public List<HexCellHash> GetAllCellsOccupiedByActor(CombatActor actor)
   {
      if (actor.IsFictional)
      {
         return null;
      }

      return GetAllCellsOccupiedForShape(actor.Position, actor.Shape);
   }*/
   
   

}
