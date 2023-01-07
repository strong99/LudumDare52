using Godot;
using System;

public class LeaveGoal : BaseGoal, HasDestinationGoal
{
    public Double Threshold { get; init; }
    public Vector2 Destination { get; init; }

    public override Boolean Finished { get => _finished; }
    private Boolean _finished;

    public LeaveGoal(String id, Vector2 destination, Double threshold = 10) : base(id)
    {
        Threshold = threshold;
        Destination = destination;
    }

    public override void Process(Double delta)
    {
        if (!Finished && Claiment.GetParent() == null)
        {
            _finished = true;
        }
    }
}
