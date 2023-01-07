using Godot;
using System;

public partial class StoryCrash : Node2D
{
    private AnimationPlayer _player;

    public override void _Ready()
    {
        base._Ready();

        _player = GetNode<AnimationPlayer>("AnimationPlayer")
            ?? throw new Exception("Unable to find animation player");

        _player.Play("Play");
        _player.AnimationFinished += _player_AnimationFinished;
    }

    private void _player_AnimationFinished(StringName animName)
    {
        GetParent<Gameplay>().SetScene("Cutscenes/StoryCollapsedCave");
    }
}
