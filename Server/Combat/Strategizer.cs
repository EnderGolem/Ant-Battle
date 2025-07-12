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
            if (_combat.Workers.ContainsKey(ant.Id))
            {
                _combat.Workers[ant.Id] = ant;
            }
        }

        List<string> antsToRemove = new List<string>();
        foreach (var ant in _combat.UnassignedAnts.Where(x => x.Value.Type != Net.Models.AntType.Worker)) 
        {
            _combat.Scouts.Add(ant.Key, ant.Value);
            antsToRemove.Add(ant.Key);
        }
        
        foreach (var ant in _combat.UnassignedAnts.Where(x => x.Value.Type == Net.Models.AntType.Worker)) 
        {
            _combat.Workers.Add(ant.Key, ant.Value);
            antsToRemove.Add(ant.Key);
        }

        foreach (var removeId in antsToRemove)
        {
            _combat.UnassignedAnts.Remove(removeId);
        }
    }
}