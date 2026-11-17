namespace TextBasedRPG.Core;

/// <summary>What the "Turn on when AFK" setting wants done.</summary>
public enum AfkAction
{
    /// <summary>Nothing.</summary>
    None,

    /// <summary>The player just went AFK and the mode is off: turn it on.</summary>
    TurnOn,

    /// <summary>The player is back, and the mode is still on only because they went AFK: turn it off.</summary>
    TurnOff,
}

/// <summary>
/// Decides what the "Turn on when AFK" setting does. The setting only applies while AFK:
/// <list type="bullet">
/// <item>The mode is off and the player goes AFK: turn it on.</item>
/// <item>The player stops being AFK: turn it off again, but only if this setting turned it on and the player hasn't
/// switched the mode since (<see cref="ModeChangedByPlayer"/>).</item>
/// <item>The mode was already on: going AFK and coming back leaves it on.</item>
/// </list>
/// Only changes count: already being AFK when watching starts doesn't turn the mode on, and turning the mode off while
/// still AFK sticks until the next time the player goes AFK.
/// </summary>
public sealed class AfkTrigger
{
    /// <summary>The game's "Away from Keyboard" online status (row of the OnlineStatus sheet).</summary>
    public const uint AfkOnlineStatus = 17;

    private bool? wasAfk;

    /// <summary>True while the mode is on because this setting turned it on.</summary>
    public bool TurnedOnByAfk { get; private set; }

    /// <summary>
    /// Feeds the current AFK state and whether the mode is on, and returns what to do. A null state (no character,
    /// e.g. during a loading screen) is ignored.
    /// </summary>
    public AfkAction Update(bool? afk, bool modeOn)
    {
        if (afk is not { } isAfk)
            return AfkAction.None;

        var before = this.wasAfk;
        this.wasAfk = isAfk;
        if (before is not { } was || was == isAfk)
            return AfkAction.None;

        if (isAfk)
            return modeOn ? AfkAction.None : AfkAction.TurnOn;

        var turnOff = this.TurnedOnByAfk && modeOn;
        this.TurnedOnByAfk = false;
        return turnOff ? AfkAction.TurnOff : AfkAction.None;
    }

    /// <summary>The mode was turned on for <see cref="AfkAction.TurnOn"/>.</summary>
    public void TurnedOn() => this.TurnedOnByAfk = true;

    /// <summary>The mode was switched some other way (by the player, a logout, a test): it's no longer ours to turn off.</summary>
    public void ModeChangedByPlayer() => this.TurnedOnByAfk = false;

    /// <summary>Forgets everything (watching stopped), so the next state is a new starting point.</summary>
    public void Reset()
    {
        this.wasAfk = null;
        this.TurnedOnByAfk = false;
    }
}
