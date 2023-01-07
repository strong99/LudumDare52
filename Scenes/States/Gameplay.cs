using Godot;
using System;

public partial class Gameplay : Node2D
{
    private Node _tracking;

    public void SetScene(String name)
    {
        if (_tracking != null)
        {
            RemoveChild(_tracking);
            _tracking.QueueFree();
        }

        AddChild(
            _tracking = ResourceLoader.Load<PackedScene>($"res://Scenes/States/{name}.tscn").Instantiate()
        );
    }
}
