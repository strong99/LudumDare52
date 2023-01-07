using Godot;
using System;

public partial class Player : Node2D, Interactable
{
    private NavigationAgent2D _agent;

    public Boolean IsDead { get => HealthPoints <= 0; }

    public Double HealthPoints { get; set; } = 10;

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
