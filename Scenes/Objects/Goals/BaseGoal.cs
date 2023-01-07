using System;

public abstract class BaseGoal : Goal
{
    public abstract Boolean Finished { get; }
    public Boolean Optional { get; init; }
    public Boolean Claimed { get => Claiment != null; }

    protected Character2D Claiment { get; private set; }

    public String Id { get; }

    public BaseGoal(String id)
    {
        Id = id;
    }

    public Goal Claim(Character2D character, Func<Goal, Boolean> filter = null)
    {
        if (Claiment != null || Finished)
            return null;

        if (filter?.Invoke(this) == false)
            return null;

        Claiment = character;

        return this;
    }

    public void UnClaim(Character2D character)
    {
        if (Claiment == character)
            Claiment = null;
        else
            throw new ArgumentException("The given character can't unclaim as it's not the claiment");
    }

    public abstract void Process(Double delta);
}
