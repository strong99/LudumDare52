using Godot;
using System;

public partial class HealthbarValue : RichTextLabel
{
    private Player _player;

    private Int32 _lastHealth;
    public override void _Process(Double delta)
    {
        _player ??= GetNode<Player>("../../../../Player");

        var p = (Int32)_player.HealthPoints;
        if (_lastHealth != p)
        {
            _lastHealth = p;
            var h = p.ToString();
            var m = ((Int32)_player.MaxHealthPoints).ToString();
            Text = $"{h}/{m}";
        }

        base._Process(delta);
    }
}
