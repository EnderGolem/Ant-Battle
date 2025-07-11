namespace Server.Combat;

public class CombatField
{
    private Dictionary<HexCellHash, HexCell> _combatField = new Dictionary<HexCellHash, HexCell>();

    public void AddHexCell(HexCellHash hash, HexCell cell)
    {
        _combatField.Add(hash,cell);
    }
}