using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

public partial class Player : Node2D, Interactable
{
    private NavigationAgent2D _agent;

    public Boolean IsDead { get => HealthPoints <= 0; }

    [Export] public Double HealthPoints { get => _healthPoints; set => _healthPoints = Math.Clamp(value, 0, MaxHealthPoints); }
    private Double _healthPoints = 10;
    [Export] public Double MaxHealthPoints { get; set; } = 10;

    [Export] public Double HungerPoints { get => _hungerPoints; set => _hungerPoints = Math.Clamp(value, 0, MaxHungerPoints); }
    private Double _hungerPoints = 10;
    [Export] public Double MaxHungerPoints { get; set; } = 10;

    public static Godot.Collections.Dictionary<Vector2, String> _animationDirections = new()
    {
        [new Vector2(1f, 0f)] = "east",
        [new Vector2(0.5f, 0.5f)] = "southeast",
        [new Vector2(0f, 1f)] = "south",
        [new Vector2(-0.5f, 0.5f)] = "southwest",
        [new Vector2(-1f, 0f)] = "west",
        [new Vector2(-0.5f, -0.5f)] = "northwest",
        [new Vector2(0f, -1f)] = "north",
        [new Vector2(0.5f, -0.5f)] = "northeast"
    };

    public Boolean IsOnTeamPlayer { get => true; }

    private AnimatedSprite2D _skin;

    public override void _Ready()
    {
        base._Ready();

        _agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _skin = GetNode<AnimatedSprite2D>("Skin");
    }

    public void GoTo(Vector2 destination)
    {
        _agent.TargetLocation = destination;
    }

    public override void _Process(System.Double delta)
    {
        if (IsDead)
            return;

        base._Process(delta);

        HungerPoints -= delta / 4;

        Vector2 next = Position;
        if (!_agent.IsNavigationFinished())
        {
            next = _agent.GetNextLocation();

            var speed = 120.0f;
            Position = Position.MoveToward(next, (Single)delta * speed);
        }

        UpdateAnim(next);
    }

    private String _previousDirection = "south";
    private void UpdateAnim(Vector2 next)
    {
        var direction = Position.DirectionTo(next);
        if (direction != Vector2.Zero)
        {

            var nearestDistance = Double.MaxValue;
            var nearest = default(string);
            foreach (var wind in _animationDirections)
            {
                var d = wind.Key.DistanceSquaredTo(direction);
                if (d < nearestDistance)
                {
                    nearestDistance = d;
                    nearest = wind.Value;
                }
            }
            _previousDirection = nearest;
        }

        if (!String.IsNullOrWhiteSpace(_previousDirection))
        {
            var animation = $"{_previousDirection}_{Action}";
            if (_skin.Animation != animation)
            {
                _skin.Play(animation);
                _skin.AnimationFinished += _skin_AnimationFinished;
            }
        }
    }

    public String Action { get; set; } = "walk";
    private void _skin_AnimationFinished()
    {
        Action = "walk";
        _skin.AnimationFinished -= _skin_AnimationFinished;
    }

    public void Damage(Double attack)
    {
        HealthPoints -= attack;
    }
}
