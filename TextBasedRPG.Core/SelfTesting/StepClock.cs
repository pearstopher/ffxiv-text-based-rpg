namespace TextBasedRPG.Core.SelfTesting;

/// <summary>
/// Decides when a self-test step has run too long. The runner calls the step once per frame; this keeps the
/// timing logic testable without the game.
/// </summary>
public sealed class StepClock
{
    private readonly long startedMs;
    private readonly long timeoutMs;

    /// <param name="startedMs">Time the step started, in milliseconds (any monotonic clock).</param>
    /// <param name="timeoutMs">How long the step may run.</param>
    public StepClock(long startedMs, long timeoutMs)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeoutMs);
        this.startedMs = startedMs;
        this.timeoutMs = timeoutMs;
    }

    /// <summary>Milliseconds since the step started.</summary>
    public long Elapsed(long nowMs) => Math.Max(0, nowMs - this.startedMs);

    /// <summary>True once the step has run for its whole time limit.</summary>
    public bool TimedOut(long nowMs) => this.Elapsed(nowMs) >= this.timeoutMs;
}
