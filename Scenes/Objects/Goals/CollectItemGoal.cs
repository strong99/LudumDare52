using Godot;
using System;

public class CollectItemGoal : BaseGoal, HasDestinationGoal, ActionGoal
{
    public Double Threshold { get; }
    public Vector2 Destination { get; }
    public Double Duration { get; }
    public Double TimeLeft { get; private set; }

    public Boolean OnLocation { get => Claiment.Position.DistanceSquaredTo(Destination) <= Threshold; }
    public override Boolean Finished { get => TimeLeft <= 0; }

    public CollectItemGoal(String id, Vector2 destination, Double gatherDuration = 3000, Double threshold = 10) : base(id)
    {
        Duration = TimeLeft = gatherDuration;
        Threshold = threshold;
        Destination = destination;
    }

    public override void Process(Double delta)
    {
        if (!Finished && OnLocation)
        {
            TimeLeft -= delta;
        }
    }
}
