using Godot;
using System;

public class SpawnFollowerGoal : BaseGoal
{
    public override Boolean Finished { get => _finished; }
    private Boolean _finished = false;

    public Vector2 Destination { get; }
    public String Type { get; }

    public SpawnFollowerGoal(String id, Vector2 position, String type) : base(id)
    {
        Destination = position;
        Type = type;
    }

    public void Spawned() { _finished = true; }
    public override void Process(Double delta)
    {
        
    }
}
