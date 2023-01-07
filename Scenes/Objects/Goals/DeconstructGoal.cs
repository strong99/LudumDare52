using Godot;
using System;

public class DeconstructGoal : BaseGoal, HasDestinationGoal
{
    public Double Threshold { get; }
    public Vector2 Destination { get; }

    public String Type { get; }
    public override Boolean Finished { get => _finished; }
    private Boolean _finished = false;

    public DeconstructGoal(String id, Vector2 destination, String type, Double threshold = 10) : base(id)
    {
        Type = type;
        Threshold = threshold;
        Destination = destination;
    }
    public override void Process(Double delta)
    {
        if (!Finished && Claiment.GetAncestor<Gameplay>().GetOnLocation<Construction>(Destination)?.Type != Type)
        {
            _finished = true;
        }
    }
}
