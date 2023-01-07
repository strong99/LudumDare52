using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class POI2D : Node2D
{
    [Export] public Array<String> Tags { get; set; }
    public Boolean Is(String tag)
    {
        return Tags?.Contains(tag) == true;
    }
}
