using Godot;
using System;

public class IdleGoal : BaseGoal, HasDestinationGoal, ActionGoal
{
    public Double Threshold { get; }
    public Vector2 Destination { get; }
    public Double Duration { get; }
    public Double TimeLeft { get; private set; }
    public Boolean IsExpired()
    {
        return TimeLeft <= 0;
    }

    public Boolean OnLocation { get => Claiment.Position.DistanceSquaredTo(Destination) <= Threshold * Threshold; }
    public override Boolean Finished { get => TimeLeft <= 0; }

    public IdleGoal(String id, Vector2 destination, Double gatherDuration = 3000, Double threshold = 10) : base(id)
    {
        Duration = TimeLeft = gatherDuration;
        Threshold = threshold;
        Destination = destination;
    }

    public override void Process(Double delta)
    {
        if (!Finished && OnLocation)
        {
            TimeLeft -= delta * 1000;
        }
    }
}

public class HideGoal : BaseGoal, HasDestinationGoal, ActionGoal
{
    public Double Threshold { get; }
    public Vector2 Destination { get; }
    public Double Duration { get; }
    public Double TimeLeft { get; private set; }
    public Boolean IsExpired()
    {
        return TimeLeft <= 0;
    }

    public Boolean OnLocation { get => Claiment.Position.DistanceSquaredTo(Destination) <= Threshold * Threshold; }
    public override Boolean Finished { get => TimeLeft <= 0; }

    public HideGoal(String id, Vector2 destination, Double gatherDuration = 3000, Double threshold = 10) : base(id)
    {
        Duration = TimeLeft = gatherDuration;
        Threshold = threshold;
        Destination = destination;
    }

    public override void Process(Double delta)
    {
        if (!Finished && OnLocation)
        {
            TimeLeft -= delta * 1000;
        }
    }
}
