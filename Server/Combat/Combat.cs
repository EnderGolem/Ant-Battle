using Server.Net.Models;

namespace Server.Combat;

public class Combat
{
    private CombatField _combatField = new CombatField();

    public MovesRequest Tick(GameState gameState)
    {
        var input = new MovesRequest();
        foreach (var tile in gameState.Map)
        {
            _combatField.AddHexCell(new HexCellHash(tile.Q, tile.R), new HexCell(tile.Cost));
        }
        
        List<Move> moves = new List<Move>();
        for (int i = 0; i < gameState.Ants.Count; i++)
        {
            Move move = new Move();
            move.Ant = gameState.Ants[i].Id;
            var antPos = new HexCellHash(gameState.Ants[i].Q, gameState.Ants[i].R);
            List<Coordinate> path = new List<Coordinate>(3);
            for (int j = 0; j < 3; j++)
            {
                path.Add((antPos + HexCellHash.Left() * j).ToCoordinate());
            }

            moves.Add(move);
        }

        input.Moves = moves;


        return input;
    }
}