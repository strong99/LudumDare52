using Godot;
using System;

public class ReturnGoal : BaseGoal
{
    public override Boolean Finished { get => _finished; }
    private Boolean _finished = false;

    public Vector2 Destination { get; }
    public PlayCave Parent { get; }

    public ReturnGoal(String id, PlayCave parent, Vector2 position) : base(id)
    {
        Destination = position;
        Parent = parent;
    }

    private void Spawned() { _finished = true; }
    public override void Process(Double delta)
    {
        if (!Finished && Claiment.Position.DistanceSquaredTo(Destination) == 1)
            _finished = true;
    }
}
