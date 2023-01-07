using Godot;
using System.Collections.Generic;
using System.Linq;

public class SharedGoal
{
    public HashSet<Character2D> Members { get; } = new();
    public Vector2 Entrance { get; set; }
    public List<Goal> Goals { get; set; } = new();
    public void Add(Goal goal)
    {
        Goals.Add(goal);
    }

    public Goal NextGoal { get => Goals.FirstOrDefault(g => !g.Finished); }
}
