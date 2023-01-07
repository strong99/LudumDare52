using System;

public interface Goal
{
    /// <summary>
    /// Attempt to claim this, or its child goal, returns null if none can be claimed
    /// </summary>
    /// <param name="character"></param>
    /// <param name="filter">An optional filter, if the character is't able to accept any goal</param>
    /// <returns></returns>
    Goal Claim(Character2D character, Func<Goal, Boolean> filter = null);

    /// <summary>
    /// Unclaims a previous goal. Useful if the character died and another is taking over.
    /// </summary>
    /// <param name="character"></param>
    void UnClaim(Character2D character);

    void Process(Double delta);

    /// <summary>
    /// Indicates whether this goal has been finished
    /// </summary>
    Boolean Finished { get; }
}
