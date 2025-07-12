namespace Server.Combat;

public class CombatField
{
    private Dictionary<HexCellHash, HexCell> _combatField = new Dictionary<HexCellHash, HexCell>();

    public Dictionary<HexCellHash, HexCell> Field => _combatField;

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
}