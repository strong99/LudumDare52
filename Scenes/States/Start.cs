using Godot;
public partial class Start : Node2D
{
    private AnimationPlayer _player;

    public override void _Ready()
    {
        _player = GetNode<AnimationPlayer>("./AnimationPlayer")
            ?? throw new System.Exception("Unable to find the animation player");

        base._Ready();

        _player.Play(AnimationReady);
    }

    private static readonly string AnimationReady = "Ready";
    private static readonly string AnimationIntroduction = "Introduction";
    public void OnStartButtonPressed()
    {
        if (_player.IsPlaying() && _player.AssignedAnimation == AnimationIntroduction)
            return;

        _player.Play(AnimationIntroduction);
    }

    public void OnStartGame()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://Scenes/States/Gameplay.tscn").Instantiate<Gameplay>();
        scene.SetScene("Cutscenes/StoryCrash");

        var parent = GetParent();
        parent.AddChild(
            scene
        );
        parent.RemoveChild(this);

        QueueFree();
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
