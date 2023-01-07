using System;

public interface ActionGoal
{
    Double Duration { get; }
    Double TimeLeft { get; }
    public Boolean IsExpired()
    {
        return TimeLeft <= 0;
    }
}
