using Godot;
using System;

public class ExternalMissionGoal : BaseGoal, HasDestinationGoal, ActionGoal
{
    public Double Threshold { get; init; }
    public Vector2 Destination { get; init; }
    public Double Duration { get; init; }
    public Double TimeLeft { get; private set; }
    public Boolean IsExpired()
    {
        return TimeLeft <= 0;
    }

    public override Boolean Finished { get => _finished; }
    private Boolean _finished;
    public Boolean OnLocation { get => Claiment.Position.DistanceSquaredTo(Destination) <= Threshold * Threshold; }

    public ExternalMissionGoal(String id, Vector2 destination, Double duration = 5000, Double threshold = 10) : base(id)
    {
        Threshold = threshold;
        Destination = destination;
        Duration = duration;
    }

    public override void Process(Double delta)
    {
        if (!Finished)
        {
            if (IsExpired())
            {
                _finished = true;
            }
            else if (Claiment.GetParent() == null)
            {
                TimeLeft -= delta;
            }
        }
    }
}
