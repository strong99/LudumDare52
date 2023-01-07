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
    Boolean IsOnTeamPlayer { get; }

    void Damage(Double attack);
}

public partial class Character2D : Node2D, Interactable
{
    public Double Controlled
    {
        get => _controlled;
        set
        {
            var preValue = _controlled;
            var newValue = _controlled = value;
            if (preValue <= 0.8 && newValue > 0.8)
                AddIcon<IconControlled>();
            if (preValue > 0.8 && newValue <= 0.8)
                RemoveIcon<IconControlled>();
        }
    }
    private Double _controlled = 0;

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
        if (target.IsOnTeamPlayer != IsOnTeamPlayer)
        {
            Alerted = Math.Clamp(Alerted + 0.5f, 0, 1);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton i && i.IsActionPressed("Do"))
        {
            var t = _skin;
            var s = t.Frames.GetFrame(t.Animation, t.Frame).GetSize();
            var r = new Rect2(new Vector2(t.Offset.x - s.x / 2, t.Offset.y - s.y / 2), s.x, s.y);
            var l = GetLocalMousePosition();
            if (r.HasPoint(l) && IsOnTeamPlayer && !IsDead)
            {
                GetAncestor<PlayCave>().ShowMenu(this);
            }
        }

        base._Input(@event);
    }

    public Vector2 Direction { get; private set; } = new();

    public Boolean IsDead { get => HealthPoints <= 0; }

    [Export] public Double HealthPoints { get => _healthPoints; set => _healthPoints = Math.Clamp(value, 0, MaxHealthPoints); }
    private Double _healthPoints = 10;
    [Export] public Double MaxHealthPoints { get; set; } = 10;

    [Export] public Double Attack { get; set; } = 1;

    [Export] public Double DetectionArea { get; set; }

    public Boolean IsOnTeamPlayer { get => Controlled >= 0.8f; }
    public Boolean IsNotOnTeamPlayer { get => Controlled < 0.8f; }
    private HashSet<Interactable> _deadBodies = new();

    /// <summary>
    /// If alerted their detection area becomes bigger
    /// </summary>
    [Export] public Double Alerted 
    { 
        get => _alerted;
        set
        {
            var preValue = _alerted;
            var newValue = _alerted = value;
            if (preValue <= 0.8 && newValue > 0.8)
                AddIcon<IconAlerted>();
            if (preValue > 0.8 && newValue <= 0.8)
                RemoveIcon<IconAlerted>();
        }
    }
    private Double _alerted = 0;

    private void AddIcon<T>() where T : Icon2D
    {
        var name = typeof(T).Name;
        var scene = ResourceLoader.Load<PackedScene>($"res://Scenes/Icons/{name}.tscn");
        var icon = scene.Instantiate<T>();
        icon.Position = new(0, -48);
        AddChild(icon);
    }

    private void RemoveIcon<T>() where T : Icon2D
    {
        foreach(var child in GetChildren())
        {
            if (child is T)
            {
                RemoveChild(child);
                break;
            }
        }
    }

    public SharedGoal Goal
    {
        get => _goal;
        set
        {
            _goal?.Members?.Remove(this);
            _goal = value;
            _goal?.Members?.Add(this);
            if (_goal == null)
            {
                _claimedGoal.UnClaim(this);
                _claimedGoal = null;
                _agent.TargetLocation = this.Position;
            }
        }
    }
    private SharedGoal _goal;

    private Goal _claimedGoal;

    private NavigationAgent2D _agent;

    private AnimatedSprite2D _skin;

    public override void _Ready()
    {
        _agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _skin = GetNode<AnimatedSprite2D>("Skin");

        base._Ready();
    }

    private Double _lastAttack = 0;

    private Dictionary<Interactable, Double> _lastSeen = new();

    private String _previousDirection = "south";
    private void UpdateAnim(Vector2 next)
    {
        var direction = Position.DirectionTo(next);
        if (direction != Vector2.Zero)
        {
            var nearestDistance = Double.MaxValue;
            var nearest = default(string);
            foreach (var wind in Player._animationDirections)
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

    public override void _Process(System.Double delta)
    {
        if (IsDead)
        {
            if (_claimedGoal != null)
            {
                _claimedGoal.UnClaim(this);
                _claimedGoal = null;
                _goal = null;
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

        Vector2 next = Vector2.Zero;

        var parent = GetParent<PlayCave>();
        if (parent != null)
        {
            var maxViewDistance = 90;
            foreach (var possibleThreat in parent.GetChildren())
            {
                if (possibleThreat is Interactable interactable &&
                    possibleThreat != this)
                {
                    var displacement = (interactable.Position - Position);
                    var n = displacement.Normalized();
                    var d = Direction.DistanceSquaredTo(n);
                    if (d < 0.5f && displacement.LengthSquared() < maxViewDistance * maxViewDistance)
                    {
                        if (interactable is Interactable i && (i.IsDead || i.IsOnTeamPlayer != IsOnTeamPlayer))
                        {
                            Alerted = 1;
                            if (i.IsDead && _deadBodies.Add(i)) Alerted += 0.2;
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
        }

        var attackInterval = 1200;
        var attackDistance = 25;
        var attackSpeed = 90.0f;
        var normalSpeed = 70.0f;

        HealthPoints += delta / 20;

        // Attack player/enemy after being warned
        if (Alerted > 0.2f && _lastSeen.Any(p => p.Key is Interactable i && i.IsOnTeamPlayer != IsOnTeamPlayer))
        {
            var target = _lastSeen.First(p => p.Key is Interactable i && i.IsOnTeamPlayer != IsOnTeamPlayer).Key as Interactable;

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
                var nextLocation = next = _agent.GetNextLocation();
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
        else if (_claimedGoal is ExternalMissionGoal exMission && exMission.OnLocation && parent != null)
        {
            parent.RemoveChild(this);
            parent.AddToMissionQue(this);
        }
        else if (_claimedGoal is ReturnGoal retMission && parent == null)
        {
            retMission.Parent.AddChild(this);
        }
        else if (_claimedGoal is SpawnFollowerGoal spnMission && parent == null)
        {
            var pack = ResourceLoader.Load<PackedScene>($"res://Scenes/Objects/{spnMission.Type}.tscn");
            var node = pack.Instantiate<Character2D>();
            node.Position = Position;
            node.Goal = new();
            node.Goal.Add(new FollowGoal("followMaster", this));
            node.Controlled = 999;
            parent.AddChild(node);
            spnMission.Spawned();
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
            if (parent.GetOnLocation<Construction>(constructGoal.Destination) == null)
            {
                parent.Construct(this, constructGoal.Destination, constructGoal.Type);
            }
        }
        else if (_claimedGoal is DeconstructGoal deconstructGoal)
        {
            if (parent.GetOnLocation<Construction>(deconstructGoal.Destination) != null)
            {
                parent.Deconstruct(this, deconstructGoal.Destination, deconstructGoal.Type);
            }
        }
        else if (_claimedGoal is LeaveGoal leaveGoal)
        {
            if (!IsOnTeamPlayer)
            {
                parent.Alertness += 0.1f * Goal.Members.Where(p => p.IsDead).Count() * (_deadBodies.Count() / 10 + 1);
                parent.AddInvestigationPoints(_deadBodies.Select(p => p.Position).ToArray());
            }

            //Goal.Members?.Remove(this);
            parent.RemoveChild(this);
            //QueueFree()
        }

        _claimedGoal?.Process(delta);

        UpdateAnim(next);

        base._Process(delta);
    }
}
