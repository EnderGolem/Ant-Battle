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

        List<string> _antsToRemove = new List<string>();
        foreach (var ant in _combat.UnassignedAnts)
        {
            _combat.Scouts.Add(ant.Key, ant.Value);
            _antsToRemove.Add(ant.Key);
        }

        foreach (var removeId in _antsToRemove)
        {
            _combat.UnassignedAnts.Remove(removeId);
        }
    }
}