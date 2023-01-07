using Godot;
using System;

public interface HasDestinationGoal
{
    Double Threshold { get; }
    Vector2 Destination { get; }

    public Boolean OnLocation(Character2D character2D) 
        => character2D.Position.DistanceSquaredTo(Destination) < Threshold * Threshold;
}
