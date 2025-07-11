using System.Collections;
using System.Collections.Generic;

public static class HexGridHelper
{
   

   private static HexCellHash[] _directions = new[]
   {
      new HexCellHash(1,0), new HexCellHash(1,-1), new HexCellHash(0,-1),
      new HexCellHash(-1,0), new HexCellHash(-1, 1), new HexCellHash(0, 1)
   };

   private static HexCellHash[] _diagonals = new[]
   {
      new HexCellHash(2,-1), new HexCellHash(1,-2), new HexCellHash(-1,-1),
      new HexCellHash(-2,1), new HexCellHash(-1, 2), new HexCellHash(1, 1)
   };
   
   public static int ManhattanDistance(HexCellHash cell1, HexCellHash cell2)
   {
      return (Math.Abs(cell1.q - cell2.q)
              + Math.Abs(cell1.q + cell1.r - cell2.q - cell2.r)
                 + Math.Abs(cell1.r - cell2.r)) / 2;
   }

   public static HexCellHash[] GetPrimaryDirections()
   {
      return _directions;
   }

   public static HexCellHash[] GetDiagonalDirections()
   {
      return _diagonals;
   }

   public static HexCellHash[] GetPrimaryNeighbors(HexCellHash cell)
   {
      var neighbors = new HexCellHash[GetPrimaryDirections().Length];

      for (int i = 0; i < GetPrimaryDirections().Length; i++)
      {
         neighbors[i] = cell + GetPrimaryDirections()[i];
      }

      return neighbors;
   }

   public static HexCellHash[] GetDiagonalNeighbors(HexCellHash cell)
   {
      var neighbors = new HexCellHash[GetDiagonalDirections().Length];

      for (int i = 0; i < GetDiagonalDirections().Length; i++)
      {
         neighbors[i] = cell + GetDiagonalDirections()[i];
      }

      return neighbors;
   }

   public static List<HexCellHash> GetAllCellsInRadius(HexCellHash center, int radius)
   {
      
      List<HexCellHash> list = new List<HexCellHash>();

      for (int q = -radius; q <= radius; q++)
      {
         for (int r = Math.Max(-radius, -q-radius); r <= Math.Min(radius, -q + radius); r++)
         {
            list.Add(center + new HexCellHash(q,r));
         }
      }
      
      return list;
   }
   

   /*public static List<HexCellHash> CalculatePath(IReadOnlyDictionary<HexCellHash,
      HexCell> field, HexCellHash startPos, HexCellHash targetPos, 
      int actorIdForCalculation = -1)
   {
      if (_astarPathfinder == null)
      {
         _astarPathfinder = new AstarPathfinder(field.Count, this);
      }

      return _astarPathfinder.Pathfind(field, startPos, targetPos, actorIdForCalculation);
   }*/

   public static List<HexCellHash> GetAllPossibleDestinationsForCell(IReadOnlyDictionary<HexCellHash, HexCell> field, 
      HexCellHash pos, int maxDistance, AstarPathfinder pathfinder,  int actorIdForCalculation = -1)
   {
      var list = GetAllCellsInRadius(pos, maxDistance);

      list.RemoveAll((cell) =>
      {
         var path = pathfinder.Pathfind(field,  pos, cell, actorIdForCalculation);
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
