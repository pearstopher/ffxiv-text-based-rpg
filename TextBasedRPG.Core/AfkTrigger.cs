namespace TextBasedRPG.Core;

/// <summary>
/// Decides when to turn the mode on because the player went AFK (the "Turn on when AFK" setting): only at the moment
/// the game marks the player AFK, not while they already were, so turning the mode off while still AFK sticks until
/// the next time they go AFK.
/// </summary>
public sealed class AfkTrigger
{
    /// <summary>The game's "Away from Keyboard" online status (row of the OnlineStatus sheet).</summary>
    public const uint AfkOnlineStatus = 17;

    private bool? wasAfk;

    /// <summary>
    /// Feeds the current AFK state (null when there's no character) and returns true when the player just went AFK.
    /// The first state seen is only a starting point, so being AFK already when watching starts doesn't count.
    /// </summary>
    public bool Update(bool? afk)
    {
        var justWentAfk = afk == true && this.wasAfk == false;
        this.wasAfk = afk;
        return justWentAfk;
    }

    /// <summary>Forgets the last state (watching stopped), so the next one is a new starting point.</summary>
    public void Reset() => this.wasAfk = null;
}
