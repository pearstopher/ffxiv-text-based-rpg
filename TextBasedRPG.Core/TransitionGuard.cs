namespace TextBasedRPG.Core;

/// <summary>
/// Decides when the 3D render flag may be changed. Changing it while the game loads a zone or plays a cutscene can
/// crash the game (XIV_HolyPlugin: <c>SetPixelShader</c> on the render thread), so changes wait until no transition
/// is in progress and a short settle time has passed. Holding the current value through a transition is fine
/// (AutoRetainer does). Based on XIV_HolyPlugin's <c>RenderingHandler.IsFlipBlocked</c> (MIT).
/// </summary>
public sealed class TransitionGuard
{
    /// <summary>How long after a transition ends before the flag may change (XIV_HolyPlugin uses the same 3 s).</summary>
    public const long SettleMs = 3_000;

    private long? transitionEndedAtMs;

    /// <summary>True when the flag may be changed this frame. Updated by <see cref="Update"/>.</summary>
    public bool CanFlip { get; private set; } = true;

    /// <summary>Call every frame with whether a zone load or cutscene is in progress.</summary>
    public bool Update(bool inTransition, long nowMs)
    {
        if (inTransition)
        {
            this.transitionEndedAtMs = nowMs;
            this.CanFlip = false;
        }
        else
        {
            this.CanFlip = this.transitionEndedAtMs is not { } ended || nowMs - ended >= SettleMs;
        }

        return this.CanFlip;
    }
}
