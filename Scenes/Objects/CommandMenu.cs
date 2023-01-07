using Godot;
using System;
using System.Linq;

public partial class CommandMenu : Node2D
{
    private Character2D _subject;
    private Label _title;

    public override void _Ready()
    {
        base._Ready();

        _title = GetNode<Label>("Panel/VBoxContainer/Title");
        ZIndex = 1000;
    }

    public void SetSubject(Character2D character)
    {
        if (character.IsDead || !character.IsOnTeamPlayer)
        {
            GetParent().RemoveChild(this);
            return;
        }

        _title ??= GetNode<Label>("Panel/VBoxContainer/Title");

        _subject = character;
        _title.Text = character.Name;
    }

    public void OnGoPreachReligion()
    {
        OnGoGatherSouls();
        Close();
    }

    private void OnGoGatherSouls()
    {
        var region = GetNode<PlayCave>("../../../");
        var pois = GetTree().GetNodesInGroup("POIs").OfType<POI2D>();
        var spawns = GetTree().GetNodesInGroup("Spawn").OfType<Node2D>();
        var spawn = region.GetRandom(spawns);

        _subject.Goal = new();
        _subject.Goal.Add(new ExternalMissionGoal("leaveRegion", spawn.Position, 5000));
        _subject.Goal.Add(new ReturnGoal("returnToRegion", region, spawn.Position));
        _subject.Goal.Add(new SpawnFollowerGoal("spawnFollower", spawn.Position, "MerchantA"));
        _subject.Goal.Add(new GoToLocationGoal("GoToCave", region.GetRandom(pois.Where(p => p.Is("InnerSanctum"))).Position));
    }

    public void OnGoFindSlaves()
    {
        OnGoGatherSouls();
        Close();
    }

    public void OnGoDefend()
    {
        var region = GetNode<PlayCave>("../../../");
        var pois = GetTree().GetNodesInGroup("POIs").OfType<POI2D>();

        _subject.Goal = new();
        _subject.Goal.Add(new GuardLocationGoal("guardCave", region.GetRandom(pois.Where(p => p.Is("Player:Defendable"))).Position));
        Close();
    }

    public void OnDistract()
    {
        _subject.Goal = new();
        _subject.Goal.Add(new WanderAndDistractGoal("wanderAndDistract"));
        Close();
    }

    private void Close()
    {
        _subject.GetAncestor<PlayCave>().ShowMenu(null);
    }
}
