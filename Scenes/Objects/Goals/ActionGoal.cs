using System;

public interface ActionGoal
{
    Double Duration { get; }
    Double TimeLeft { get; }
    Boolean IsExpired();
}
