namespace TextBasedRPG.Core;

/// <summary>A request to change the mode.</summary>
public enum ModeRequest
{
    /// <summary>Switch it.</summary>
    Toggle,

    /// <summary>Turn it on.</summary>
    On,

    /// <summary>Turn it off.</summary>
    Off,
}

/// <summary>Result of a mode request.</summary>
public enum ModeChange
{
    /// <summary>The mode turned on.</summary>
    TurnedOn,

    /// <summary>The mode turned off.</summary>
    TurnedOff,

    /// <summary>Already in the requested state; nothing changed.</summary>
    Unchanged,

    /// <summary>Refused: the mode needs a logged-in character (the chat only exists in game).</summary>
    RefusedNotLoggedIn,
}

/// <summary>
/// Whether text-only mode is on. Always starts off (nothing is remembered across loads), can only turn on with
/// a logged-in character, and is forced off on logout.
/// </summary>
public sealed class ModeState
{
    /// <summary>True while the mode is on.</summary>
    public bool IsOn { get; private set; }

    /// <summary>Applies a player's request.</summary>
    public ModeChange Request(ModeRequest request, bool loggedIn)
    {
        var wantOn = request switch
        {
            ModeRequest.On => true,
            ModeRequest.Off => false,
            _ => !this.IsOn,
        };

        if (wantOn == this.IsOn)
            return ModeChange.Unchanged;
        if (wantOn && !loggedIn)
            return ModeChange.RefusedNotLoggedIn;

        this.IsOn = wantOn;
        return wantOn ? ModeChange.TurnedOn : ModeChange.TurnedOff;
    }

    /// <summary>The character logged out: the mode turns off.</summary>
    public ModeChange LoggedOut()
    {
        if (!this.IsOn)
            return ModeChange.Unchanged;

        this.IsOn = false;
        return ModeChange.TurnedOff;
    }
}
