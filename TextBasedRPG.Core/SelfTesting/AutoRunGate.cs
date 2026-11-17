namespace TextBasedRPG.Core.SelfTesting;

/// <summary>What the auto-run should do this frame.</summary>
public enum AutoRunDecision
{
    /// <summary>Keep waiting.</summary>
    Wait,

    /// <summary>Start the self-tests now.</summary>
    Start,

    /// <summary>Waited too long without a logged-in character; stop trying.</summary>
    GiveUp,
}

/// <summary>
/// Decides when the dev build's automatic self-test run starts after a load: once a character has been logged in for
/// a few seconds (so the login burst settles), giving up after a few minutes.
/// </summary>
public sealed class AutoRunGate
{
    /// <summary>How long the character must be logged in before the run starts.</summary>
    public const long SettleMs = 3_000;

    /// <summary>How long to wait for a login before giving up.</summary>
    public const long GiveUpMs = 5 * 60_000;

    private readonly long loadedAtMs;
    private long? loggedInSinceMs;

    /// <param name="loadedAtMs">Time the plugin loaded (monotonic milliseconds).</param>
    public AutoRunGate(long loadedAtMs) => this.loadedAtMs = loadedAtMs;

    /// <summary>Call regularly with the current time and login state.</summary>
    public AutoRunDecision Update(long nowMs, bool loggedIn)
    {
        if (!loggedIn)
        {
            this.loggedInSinceMs = null;
            return nowMs - this.loadedAtMs >= GiveUpMs ? AutoRunDecision.GiveUp : AutoRunDecision.Wait;
        }

        this.loggedInSinceMs ??= nowMs;
        return nowMs - this.loggedInSinceMs.Value >= SettleMs ? AutoRunDecision.Start : AutoRunDecision.Wait;
    }
}
