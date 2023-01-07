using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Wave
{
    public HashSet<String> Participants { get; }
    public Double Interval { get; } = 4000;
    public Double InitialDelay { get; } = 0;
    public Boolean SetupCamp { get; } = false;
    public Wave(Double initialDelay, Double interval, Boolean setupCamp, params String[] participants)
    {
        InitialDelay = initialDelay;
        Interval = interval;
        SetupCamp = setupCamp;
        Participants = participants.ToHashSet();
    }
}

public partial class PlayCave : Node2D
{
    private Player _player;
    private Camera2D _camera;
    private Double _timePassed = 0;

    private HashSet<Wave> _waves = new()
    {
        new Wave(1000, 15000, false, "MerchantA", "MerchantB"),
        new Wave(1200, 15000, true, "MerchantA", "MerchantB"),
        new Wave(15000, 17000, false, "MerchantA", "PriestA"),
        new Wave(4500, 21000, true, "PriestB", "PriestA"),
        new Wave(12000, 81000, true, "GuardA", "PriestA"),
        new Wave(23000, 81000, true, "GuardA", "GuardB", "PriestA")
    };

    public override void _Ready()
    {
        base._Ready();

        _player = ResourceLoader.Load<PackedScene>("res://Scenes/Objects/Player.tscn").Instantiate<Player>();
        _player.YSortEnabled = true;
        _camera = GetNode<Camera2D>("Camera2D");
        _camera.Current = true;
        AddChild(_player);
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("Do"))
        {
            var mousePosition = GetGlobalMousePosition();
            var tileMap = GetNode<TileMap>("TileMap");
            var tileCoords = tileMap.LocalToMap(ToLocal(mousePosition));
            var tileId = tileMap.GetCellSourceId(0, tileCoords);
            var tile = tileMap.GetCellTileData(0, tileCoords);
            if (tile.GetNavigationPolygon(0) != null)
            {
                _player.GoTo(mousePosition);
            }
        }
        base._Input(inputEvent);
    }

    public override void _Process(Double delta)
    {
        var previoustimePassed = _timePassed;
        _timePassed += delta * 1000;
        _camera.Position = _camera.Position.MoveToward(_player.Position, (Single)delta * 35f);

        TryNextSpawn(previoustimePassed, _timePassed);

        base._Process(delta);
    }

    private void TryNextSpawn(Double previousTime, Double currentTime)
    {
        var poiNodes = GetTree().GetNodesInGroup("POIs").OfType<Node2D>();
        var spawnNodes = GetTree().GetNodesInGroup("Spawn").OfType<Node2D>();
        foreach (var wave in _waves)
        {
            var p = (previousTime - wave.InitialDelay) / wave.Interval;
            var n = (currentTime - wave.InitialDelay) / wave.Interval;
            var t = Math.Ceiling(p);
            if (n >= t)
            {
                Debug.WriteLine($"Spawn group: {String.Join("; ",wave.Participants)}");
                var spawn = spawnNodes.First();

                var group = new SharedGoal();
                foreach (var participant in wave.Participants)
                {
                    var path = $"res://Scenes/Objects/{participant}.tscn";
                    Debug.WriteLine(path);
                    var character = ResourceLoader.Load<PackedScene>(path).Instantiate<Character2D>();
                    AddChild(character);
                    character.Goal = group;
                    character.Position = spawn.Position;
                    group.Entrance = spawn.Position;

                    if (wave.SetupCamp)
                    {
                        group.Add(new GoalGroup("PrepareCamp").Add(
                                new ConstructGoal("SetupCamp", poiNodes.First().Position, "tent"),
                                new CollectItemGoal("CollectWater", poiNodes.First().Position),
                                new CollectItemGoal("CollectWood", poiNodes.First().Position))
                        );
                        group.Add(new IdleGoal("sleep", poiNodes.First().Position, 5000));
                        group.Add(new DeconstructGoal("PrepareDeparture", poiNodes.First().Position, "tent"));
                    }
                    group.Add(new LeaveGoal("leave", spawnNodes.First(p => p.Position != group.Entrance).Position));
                }
            }
        }
    }

    public T GetOnLocation<T>(Vector2 position)
    {
        var tileMap = GetNode<TileMap>("TileMap");
        var localCoords = tileMap.ToLocal(position);
        var tileCoords = tileMap.LocalToMap(localCoords);

        return default;
    }
}
