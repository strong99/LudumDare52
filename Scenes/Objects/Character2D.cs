using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

public interface Interactable
{
    public StringName Name { get; }
    public Vector2 Position { get; }
    public Vector2 GlobalPosition { get; }
    Double HealthPoints { get; }
    Boolean IsDead { get; }
    void Damage(Double attack);
}

public partial class Character2D : Node2D, Interactable
{
    public Double Controlled { get; set; } 
    public T GetAncestor<T>()
    {
        var parent = GetParent();
        while (parent is not T && parent != null)
        {
            parent = parent.GetParent();
        }

        if (parent is T t)
            return t;

        throw new Exception();
    }

    public void Damage(Double attack)
    {
        HealthPoints -= attack;
    }

    public void Alert(Interactable target)
    {
        Alerted = Math.Clamp(Alerted + 0.5f, 0, 1);
    }

    public Vector2 Direction { get; private set; } = new();

    public Boolean IsDead { get => HealthPoints <= 0; }

    [Export] public Double HealthPoints { get; set; } = 10;

    [Export] public Double Attack { get; set; }

    [Export] public Double DetectionArea { get; set; }

    /// <summary>
    /// If alerted their detection area becomes bigger
    /// </summary>
    [Export] public Double Alerted { get; set; }

    public SharedGoal Goal
    {
        get => _goal;
        set
        {
            _goal?.Members?.Remove(this);
            _goal = value;
            _goal?.Members?.Add(this);
        }
    }
    private SharedGoal _goal;

    private Goal _claimedGoal;

    private NavigationAgent2D _agent;

    public override void _Ready()
    {
        _agent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        base._Ready();
    }

    private Double _lastAttack = 0;

    private Dictionary<Interactable, Double> _lastSeen = new();

    public override void _Process(System.Double delta)
    {
        if (IsDead)
        {
            if (_claimedGoal != null)
            {
                _claimedGoal.UnClaim(this);
                _claimedGoal = null;
            }
            return;
        }

        if (Alerted > 0.2)
        {
            Alerted -= delta / 2.0;
            if (Alerted < 0.2) Alerted = 0.2;
        }

        _claimedGoal ??= Goal?.NextGoal?.Claim(this);

        var keys = _lastSeen.Keys.ToArray();
        foreach (var key in keys)
        {
            var value = _lastSeen[key];
            _lastSeen[key] = value + delta * 1000;
            if (value > 4000)
                _lastSeen.Remove(key);
        }

        var maxViewDistance = 90;
        Debug.WriteLine("----");
        foreach (var possibleThreat in GetParent().GetChildren())
        {
            if (possibleThreat is Interactable interactable &&
                possibleThreat != this)
            {
                var displacement = (interactable.Position - Position);
                var n = displacement.Normalized();
                var d = Direction.DistanceSquaredTo(n);
                if (d < 0.5f && displacement.LengthSquared() < maxViewDistance * maxViewDistance)
                {
                    if (interactable is Player)
                    {
                        Alerted = 1;
                        if (Goal != null && Goal.Members.Any())
                        {
                            foreach (var m in Goal.Members)
                                m.Alert(interactable);
                        }
                    }
                    if (_lastSeen.ContainsKey(interactable)) _lastSeen[interactable] = 0;
                    else _lastSeen.Add(interactable, 0);
                }
            }
        }

        var attackInterval = 1200;
        var attackDistance = 15;
        var attackSpeed = 90.0f;
        var normalSpeed = 70.0f;

        // Attack player/enemy after being warned
        if (Alerted > 0.2f && _lastSeen.Any(p => p.Key is Player && Controlled < 0.8 || Controlled >= 0.9 && p.Key is Character2D character && character.Controlled < 0.8))
        {
            var target = _lastSeen.First(p => p.Key is Player && Controlled < 0.8 || Controlled >= 0.9 && p.Key is Character2D character && character.Controlled < 0.8).Key as Interactable;

            _lastAttack += delta * 1000;
            if (target.Position.DistanceSquaredTo(Position) < attackDistance * attackDistance)
            {
                if (_lastAttack > attackInterval)
                {
                    target.Damage(Attack);
                    _lastAttack = 0;
                }
            }
            else
            {
                if (_agent.TargetLocation.DistanceSquaredTo(target.Position) > 100)
                {
                    _agent.TargetLocation = target.Position;
                    _agent.TargetDesiredDistance = attackDistance * 0.8f;
                }
                var nextLocation = _agent.GetNextLocation();
                Direction = (nextLocation - Position).Normalized();
                Position = Position.MoveToward(nextLocation, (Single)delta * attackSpeed);
            }
        }
        // Continue goals
        else if (_claimedGoal?.Finished == true)
        {
            _claimedGoal.UnClaim(this);
            _claimedGoal = null;
        }
        else if (_claimedGoal is HasDestinationGoal hasDestinationGoal && !hasDestinationGoal.OnLocation(this))
        {
            if (_agent.TargetLocation != hasDestinationGoal.Destination)
            {
                _agent.TargetLocation = hasDestinationGoal.Destination;
                _agent.TargetDesiredDistance = (Single)hasDestinationGoal.Threshold;
            }

            var nextLocation = _agent.GetNextLocation();
            Direction = (nextLocation - Position).Normalized();
            Position = Position.MoveToward(nextLocation, (Single)delta * normalSpeed);
        }
        else if (_claimedGoal is HideGoal hideGoal && ((
            !hideGoal.IsExpired() && Visible
        ) || (
            hideGoal.IsExpired() && !Visible
        )))
        {
            Visible = !hideGoal.IsExpired();
        }
        else if (_claimedGoal is ActionGoal actionGoal && !actionGoal.IsExpired())
        {
            // Play animation?
        }
        else if (_claimedGoal is ConstructGoal constructGoal)
        {
            if (GetAncestor<Gameplay>().GetOnLocation<Construction>(constructGoal.Destination) == null)
            {
                GetParent<PlayCave>().Construct(this, constructGoal.Destination, constructGoal.Type);
            }
        }
        else if (_claimedGoal is DeconstructGoal deconstructGoal)
        {
            if (GetAncestor<Gameplay>().GetOnLocation<Construction>(deconstructGoal.Destination) != null)
            {
                GetParent<PlayCave>().Deconstruct(this, deconstructGoal.Destination, deconstructGoal.Type);
            }
        }
        else if (_claimedGoal is LeaveGoal leaveGoal)
        {
            //Goal.Members?.Remove(this);
            GetParent().RemoveChild(this);
            //QueueFree()

            var playCave = GetAncestor<PlayCave>();
            playCave.Alertness += 0.1f * Goal.Members.Count;
            playCave.AddInvestigationPoints(Goal.Members.Where(p=>p.IsDead).Select(p=>p.Position).ToArray());
        }

        _claimedGoal?.Process(delta);

        base._Process(delta);
    }
}
