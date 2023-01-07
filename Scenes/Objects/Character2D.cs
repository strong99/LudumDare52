using Godot;
using System;

public partial class Character2D : Node2D
{
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

    public override void _Process(System.Double delta)
    {
        _claimedGoal ??= Goal.NextGoal.Claim(this);

        if (_claimedGoal?.Finished == true)
        {
            _claimedGoal.UnClaim(this);
            _claimedGoal = null;
        }
        else if (_claimedGoal is HasDestinationGoal hasDestinationGoal && !hasDestinationGoal.OnLocation(this))
        {
            var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
            if (agent.TargetLocation != hasDestinationGoal.Destination)
            {
                agent.TargetLocation = hasDestinationGoal.Destination;
                agent.TargetDesiredDistance = (Single)hasDestinationGoal.Threshold;
            }

            var speed = 30.0f;
            Position = Position.MoveToward(agent.GetNextLocation(), (Single)delta * speed);
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
            Goal.Members?.Remove(this);
            GetParent().RemoveChild(this);
            QueueFree();
        }

        _claimedGoal?.Process(delta);

        base._Process(delta);
    }
}
