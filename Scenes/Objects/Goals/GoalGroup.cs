using System;
using System.Collections.Generic;
using System.Linq;

public class GoalGroup : Goal
{
    private readonly HashSet<Goal> _goals = new();

    public Boolean Finished { get => _goals.All(g=> g.Finished || (!g.Claimed && g.Optional)); }

    public String Id { get; }
    public Boolean Claimed { get => _goals.Any(g => g.Claimed); }
    public Boolean Optional { get; init; }

    public GoalGroup(String id)
    {
        Id = id;
    }

    public GoalGroup Add(params Goal[] goals)
    {
        foreach(var goal in goals)
        {
            _goals.Add(goal);
        }
        return this;
    }

    public Goal Claim(Character2D character, Func<Goal, Boolean> filter = null)
    {
        var goal = default(Goal);
        foreach(var g in _goals)
        {
            goal = g.Claim(character, filter);
            if (goal != null) break;
        }
        return goal;
    }

    public void UnClaim(Character2D character)
    {
        throw new NotImplementedException();
    }

    public void Process(Double delta)
    {
        foreach(var g in _goals)
        {
            g.Process(delta);
        }
    }
}
