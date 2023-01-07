using Godot;
using System;

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

    public Boolean IsOnTeamPlayer { get => true; }

    public override void _Ready()
    {
        base._Ready();

        _agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
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

        if (!_agent.IsNavigationFinished())
        {
            var next = _agent.GetNextLocation();

            var speed = 120.0f;
            Position = Position.MoveToward(next, (Single)delta * speed);
        }
    }

    public void Damage(Double attack)
    {
        HealthPoints -= attack;
    }
}
