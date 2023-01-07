using Godot;
using System;

public partial class WorldAlertnessbarValue : RichTextLabel
{
    private PlayCave _cave;

    private Int32 _lastLevel;
    public override void _Process(Double delta)
    {
        _cave ??= GetNode<PlayCave>("../../../../../");

        var p = (Int32)_cave.Alertness;
        if (_lastLevel != p)
        {
            _lastLevel = p;
            Text = p.ToString();
        }

        base._Process(delta);
    }
}
