using Server.Net.Models;

namespace Server.Combat;

public class CombatField
{
    private Dictionary<HexCellHash, HexCell> _combatField = new Dictionary<HexCellHash, HexCell>();
    private Dictionary<HexCellHash, FoodOnMap> _foodFields = new Dictionary<HexCellHash, FoodOnMap>();

    public Dictionary<HexCellHash, HexCell> Field => _combatField;

    public Dictionary<HexCellHash, FoodOnMap> Foods => _foodFields;

    public void SetHexCell(HexCellHash hash, HexCell cell)
    {
        _combatField[hash] = cell;
    }

    public void AddFakeCell(HexCellHash hash, HexCell cell)
    {
        if (_combatField.ContainsKey(hash))
        {
            return;
        }

        _combatField[hash] = cell;
    }  
    
    
    public void AddFood(HexCellHash hash, FoodOnMap food)
    {
        if (_foodFields.ContainsKey(hash))
        {
            return;
        }

        _foodFields[hash] = food;
    } 
    
    public void Remove(HexCellHash hash)
    {
        _foodFields.Remove(hash);
    }
}