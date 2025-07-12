namespace Server.Combat;

public class Strategizer
{
    private Combat _combat;
    
    public Strategizer(Combat combat)
    {
        _combat = combat;
    }

    public void Strategize()
    {
        foreach (var ant in _combat.CurrentGameState.Ants)
        {
            if (_combat.Scouts.ContainsKey(ant.Id))
            {
                _combat.Scouts[ant.Id] = ant;
            }
        }

        foreach (var ant in _combat.UnassignedAnts)
        {
            
        }
    }
}