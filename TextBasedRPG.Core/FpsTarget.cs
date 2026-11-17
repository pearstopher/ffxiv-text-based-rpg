namespace TextBasedRPG.Core;

/// <summary>
/// Picks the frame-rate cap while the mode is on: the low idle cap normally, a higher cap while the player is typing
/// (and briefly after), so typing in chat stays responsive.
/// </summary>
public sealed class FpsTarget
{
    /// <summary>How long the typing cap lasts after the chat input closes (e.g. after pressing Enter).</summary>
    public const long LingerMs = 3_000;

    /// <summary>Default cap while idle.</summary>
    public const int DefaultIdleFps = 10;

    /// <summary>Default cap while typing.</summary>
    public const int DefaultTypingFps = 30;

    private long? lastTypingMs;

    /// <summary>
    /// Call every frame. Returns <paramref name="typingFps"/> while typing and for <see cref="LingerMs"/> after,
    /// otherwise <paramref name="idleFps"/>. The typing cap is never lower than the idle cap.
    /// </summary>
    public int Choose(int idleFps, int typingFps, bool typing, long nowMs)
    {
        if (typing)
            this.lastTypingMs = nowMs;

        var recentlyTyped = this.lastTypingMs is { } last && nowMs - last < LingerMs;
        return recentlyTyped ? Math.Max(idleFps, typingFps) : idleFps;
    }

    /// <summary>Forgets recent typing (e.g. when the mode turns on).</summary>
    public void Reset() => this.lastTypingMs = null;
}
