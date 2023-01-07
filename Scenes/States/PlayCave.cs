using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;

class Wave
{
    public String[] Participants { get; }
    public Double Interval { get; } = 4000;
    public Double InitialDelay { get; } = 0;
    public Boolean SetupCamp { get; } = false;
    public Wave(Double initialDelay, Double interval, Boolean setupCamp, params String[] participants)
    {
        InitialDelay = initialDelay;
        Interval = interval;
        SetupCamp = setupCamp;
        Participants = participants.ToArray();
    }
}

public partial class PlayCave : Node2D
{
    private Player _player;
    private Camera2D _camera;
    private Double _timePassed = 0;

    private HashSet<Character2D> _missions = new();

    private HashSet<Wave> _waves = new()
    {
        new Wave(1000, 40000, true, "MerchantA", "MerchantA", "MerchantA"),
        new Wave(1000, 20000, false, "AdventurorA"),
        //new Wave(1000, 20000, false, "MerchantA", "MerchantB"),
        //new Wave(1200, 60000, true, "MerchantA", "MerchantB"),
        //new Wave(15000, 17000, false, "MerchantA", "PriestA"),
        //new Wave(4500, 21000, true, "PriestB", "PriestA"),
        //new Wave(12000, 81000, true, "GuardA", "PriestA"),
        //new Wave(23000, 81000, true, "GuardA", "GuardB", "PriestA")
    };

    public override void _Ready()
    {

        _player = ResourceLoader.Load<PackedScene>("res://Scenes/Objects/Player.tscn").Instantiate<Player>();
        _player.YSortEnabled = true;
        _camera = GetNode<Camera2D>("Camera2D");
        _camera.Current = true;
        AddChild(_player);

        base._Ready();
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
            if (tile?.GetNavigationPolygon(0) != null)
            {
                _player.GoTo(mousePosition);
            }
        }
        var tryControl = inputEvent.IsActionPressed("Control");
        var tryHarvest = inputEvent.IsActionPressed("Harvest");
        if (tryControl || tryHarvest) 
        {
            //find nearest character
            var children = GetChildren();
            foreach(var child in children)
            {
                if (child is Character2D character && !character.IsDead && character.Controlled < 1 && character.Position.DistanceSquaredTo(_player.Position) < 200)
                {
                    if (tryControl)
                    {
                        character.Controlled += 0.2;
                        if (character.Controlled >= 1)
                        {
                            character.Goal = null;
                            _player.HungerPoints -= 0.1;
                        }
                    }
                    else if (tryHarvest)
                    {
                        character.Damage(2);
                        if (character.IsDead)
                        {
                            _player.HungerPoints += 1.1;
                            _player.HealthPoints += 0.1;
                            Debug.WriteLine("Harvested!");
                        }
                    }

                    break;
                }
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

        foreach(var m in _missions)
        {
            m._Process(delta);
        }

        base._Process(delta);
    }

    private Random _random = new();

    public Double Alertness { get; set; }
    public HashSet<Vector2> InvestigationSpots { get; } = new();

    public void AddInvestigationPoints(params Vector2[] points)
    {
        foreach (var point in points)
            InvestigationSpots.Add(point);

        if (InvestigationSpots.Count > 5)
            InvestigationSpots.Remove(InvestigationSpots.First());
    }

    private void TryNextSpawn(Double previousTime, Double currentTime)
    {
        var poiNodes = GetTree().GetNodesInGroup("POIs").OfType<POI2D>();
        var spawnNodes = GetTree().GetNodesInGroup("Spawn").OfType<Node2D>();
        foreach (var wave in _waves)
        {
            var p = (previousTime - wave.InitialDelay) / wave.Interval;
            var n = (currentTime - wave.InitialDelay) / wave.Interval;
            var t = Math.Ceiling(p);
            if (n >= t)
            {
                Debug.WriteLine($"Spawn group: {String.Join("; ", wave.Participants)}");
                var spawn = spawnNodes.First();

                var group = new SharedGoal();
                if (wave.SetupCamp)
                {
                    var campSite = GetRandom(poiNodes.Where(p => p.Is("Campsite")));
                    var intermediateRoadNode = GetRandom(poiNodes.Where(p => p.Is("Intermediate")));
                    group.Add(new GoalGroup("PrepareCamp").Add(
                            new GoToLocationGoal("goto", Jitter(intermediateRoadNode.Position)),
                            new GoToLocationGoal("goto", Jitter(intermediateRoadNode.Position)) { Optional = true },
                            new GoToLocationGoal("goto", Jitter(intermediateRoadNode.Position)) { Optional = true },
                            new GoToLocationGoal("goto", Jitter(intermediateRoadNode.Position)) { Optional = true }
                    ));
                    // Add adventuror interest spots
                    if (InvestigationSpots.Count > 0 && Alertness > 0.5f) 
                    {
                        var g = (group.Goals.First() as GoalGroup);
                        foreach (var i in InvestigationSpots)
                            g.Add(new GoToLocationGoal("investigate", i) { Optional = true });
                    }
                    group.Add(new GoalGroup("PrepareCamp").Add(
                            new ConstructGoal("SetupCamp", campSite.Position, "Tent", 70),
                            new CollectItemGoal("CollectWater", GetRandom(poiNodes.Where(p => p.Is("Water"))).Position),
                            new CollectItemGoal("CollectWater", GetRandom(poiNodes.Where(p => p.Is("BerryBush"))).Position),
                            new CollectItemGoal("CollectWood", GetRandom(poiNodes.Where(p => p.Is("Wood"))).Position))
                    );
                    group.Add(new GoalGroup("Sleep").Add(
                        new HideGoal("sleep", campSite.Position, 5000),
                        new IdleGoal("guard", poiNodes.First().Position, 5000),
                        new HideGoal("sleep", campSite.Position, 5000) { Optional = true },
                        new HideGoal("sleep", campSite.Position, 5000) { Optional = true },
                        new HideGoal("sleep", campSite.Position, 5000) { Optional = true }
                    ));
                    group.Add(new DeconstructGoal("PrepareDeparture", campSite.Position, "Tent"));
                }
                group.Entrance = spawn.Position;
                group.Add(new GoalGroup("Leave").Add(
                    new LeaveGoal("leave", GetRandom(spawnNodes.Where(p => p.Position != group.Entrance)).Position),
                    new LeaveGoal("leave", GetRandom(spawnNodes.Where(p => p.Position != group.Entrance)).Position) { Optional = true },
                    new LeaveGoal("leave", GetRandom(spawnNodes.Where(p => p.Position != group.Entrance)).Position) { Optional = true },
                    new LeaveGoal("leave", GetRandom(spawnNodes.Where(p => p.Position != group.Entrance)).Position) { Optional = true }
                ));

                foreach (var participant in wave.Participants)
                {
                    var path = $"res://Scenes/Objects/{participant}.tscn";
                    Debug.WriteLine(path);
                    var character = ResourceLoader.Load<PackedScene>(path).Instantiate<Character2D>();
                    AddChild(character);
                    character.Goal = group;
                    character.Position = spawn.Position;
                }
            }
        }
    }

    private Vector2 Jitter(Vector2 input, Single offset = 40)
    {
        return new Vector2(input.x + ((Single)_random.NextDouble() - 0.5f) * offset, input.y + ((Single)_random.NextDouble() - 0.5f) * offset);
    }

    public T GetRandom<T>(IEnumerable<T> collection)
    {
        return collection.Skip(_random.Next(collection.Count())).First();
    }

    public T GetOnLocation<T>(Vector2 position) where T : Node2D
    {
        var tileMap = GetNode<TileMap>("TileMap");
        var localCoords = tileMap.ToLocal(position);
        var tileCoords = tileMap.LocalToMap(localCoords);

        foreach (var child in tileMap.GetChildren())
        {
            if (child is T c)
            {
                var tilePosition = tileMap.LocalToMap(c.Position);
                var tileId = tileMap.GetCellSourceId(1, tilePosition);

                if (tileMap.LocalToMap(c.Position) == tileCoords)
                {
                    return c;
                }
            }
        }

        return default;
    }

    public void Construct(Character2D character2D, Vector2 position, String type)
    {
        var tileMap = GetNode<TileMap>("TileMap");
        var tileMapCoords = tileMap.ToLocal(position);
        var tileCoords = tileMap.LocalToMap(tileMapCoords);

        var c = ResourceLoader.Load<PackedScene>($"res://Scenes/Objects/{type}.tscn").Instantiate<Construction>();
        c.Position = tileMap.MapToLocal(tileCoords);
        tileMap.AddChild(c);
    }

    public void Deconstruct(Character2D character2D, Vector2 position, String type)
    {
        var construction = GetOnLocation<Construction>(position);
        if (construction == null)
            return;

        construction.GetParent().RemoveChild(construction);
        construction.QueueFree();
    }

    public void AddToMissionQue(Character2D character2D)
    {
        _missions.Add(character2D);
    }

    private CommandMenu _menu;
    public void ShowMenu(Character2D character2D)
    {
        var camera = GetNode < Camera2D >("Camera2D");

        if (_menu != null)
            camera.RemoveChild(_menu);

        if (character2D != null)
        {
            var pack = ResourceLoader.Load<PackedScene>("res://Scenes/Objects/CommandMenu.tscn");
            _menu = pack.Instantiate<CommandMenu>();
            _menu.SetSubject(character2D);
            _menu.Position = new(0, -64);

            camera.AddChild(_menu);
        }
    }
}
