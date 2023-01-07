using Godot;
using System;

public class GuardLocationGoal : BaseGoal, HasDestinationGoal
{
    public override Boolean Finished { get => false; }

    public Double Threshold { get; }

    public Vector2 Destination { get; }

    public GuardLocationGoal(String id, Vector2 position, Int32 threshold = 10) : base(id)
    {
        Destination = position;
        Threshold = threshold;
    }

    public override void Process(Double delta)
    {

    }
}
