using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatField 
{
    private Dictionary<HexCellHash, HexCell> _combatField;

    public Dictionary<HexCellHash, HexCell> Field => _combatField;
    
    public Dictionary<HexCellHash, HexCell> GenerateCircularField(HexGridHelper helper, int radius)
    {
        var dict = new Dictionary<HexCellHash, HexCell>();
        var positions = helper.GetAllCellsInRadius(HexCellHash.Zero(), radius);
        foreach (var hash in positions)
        {
            var hexCell = new HexCell();
            if (helper.ManhattanDistance(HexCellHash.Zero(), hash) == radius)
            {
                if (hash.q < 0)
                {
                    hexCell.DeploymentPlayerId = 1;
                    hexCell.DeploymentDirection = HexCellHash.Right();
                }
                else if(hash.q > 0)
                {
                    hexCell.DeploymentPlayerId = 2;
                    hexCell.DeploymentDirection = HexCellHash.Left();
                }
            }

            dict[hash] = hexCell;
        }

        _combatField = dict;

        return _combatField;
    }
}
