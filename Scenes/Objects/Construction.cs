using Godot;
using System;

public partial class Construction : Node2D
{
    [Export] public String Type { get; set; }
    [Export] public Int32 Width { get; set; }
    [Export] public Int32 Height { get; set; }
}