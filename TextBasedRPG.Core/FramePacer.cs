namespace TextBasedRPG.Core;

/// <summary>
/// How long to sleep at the end of a frame to hold a target frame rate. Based on ChillFrames'
/// <c>FrameLimiterController.PerformLimiting</c> (MidoriKami, AGPL-3.0), without its busy-wait: precision doesn't
/// matter here and spinning burns the CPU this plugin is trying to save.
/// </summary>
public static class FramePacer
{
    /// <summary>Lowest allowed target.</summary>
    public const int MinFps = 1;

    /// <summary>Highest allowed target.</summary>
    public const int MaxFps = 60;

    /// <summary>
    /// Whole milliseconds to sleep so the frame lasts about 1000 / <paramref name="targetFps"/> ms.
    /// </summary>
    /// <param name="targetFps">Target frame rate; 0 or less means "no cap" (returns 0). Clamped to 1–60.</param>
    /// <param name="elapsedMs">Time since the previous frame ended (the work this frame already took).</param>
    public static int SleepMs(int targetFps, double elapsedMs)
    {
        if (targetFps <= 0 || double.IsNaN(elapsedMs))
            return 0;

        var frameMs = 1000.0 / Math.Clamp(targetFps, MinFps, MaxFps);
        var remaining = frameMs - Math.Max(0, elapsedMs);

        // Round down: Thread.Sleep tends to oversleep by up to a timer tick, so shorter is closer to the target.
        return remaining >= 1 ? (int)remaining : 0;
    }
}
