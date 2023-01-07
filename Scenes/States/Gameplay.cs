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

    public T GetOnLocation<T>(Vector2 position)
    {
        if (_tracking is PlayCave playCave)
        {
            return playCave.GetOnLocation<T>(position);
        }
        throw new NotImplementedException();
    }
}
