using Godot;
using System;

public partial class HungerbarValue : RichTextLabel
{
    private Player _player;

    private Int32 _lastvalue;
    public override void _Process(Double delta)
    {
        _player ??= GetNode<Player>("../../../../Player");

        var p = (Int32)_player.HungerPoints;
        if (_lastvalue != p)
        {
            _lastvalue = p;
            var h = p.ToString();
            var m = ((Int32)_player.MaxHungerPoints).ToString();
            Text = $"{h}/{m}";
        }

        base._Process(delta);
    }
}
