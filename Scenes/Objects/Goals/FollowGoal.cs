using Godot;
using System;

public class FollowGoal : BaseGoal, HasDestinationGoal
{
    public Double Threshold { get; init; }
    public Vector2 Destination { get => _target.Position; }
    private Interactable _target;

    public override Boolean Finished { get => false; }
    public Boolean OnLocation { get => Claiment.Position.DistanceSquaredTo(Destination) <= Threshold * Threshold; }

    public FollowGoal(String id, Interactable target, Double threshold = 10) : base(id)
    {
        Threshold = threshold;
        _target = target;
    }

    public override void Process(Double delta)
    {

    }
}
