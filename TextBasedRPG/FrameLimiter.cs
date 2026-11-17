using System;
using System.Diagnostics;
using System.Threading;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Caps the game's frame rate by sleeping at the end of each framework update, the technique ChillFrames (MidoriKami,
/// AGPL-3.0) uses, minus its busy-wait. With 3D off, frames are so cheap that an uncapped game would run hundreds of
/// them per second; the cap is where most of the CPU saving comes from. See docs/design/research.md §2.
/// </summary>
internal sealed class FrameLimiter : IDisposable
{
    private readonly Stopwatch sinceLastFrame = Stopwatch.StartNew();

    internal FrameLimiter()
    {
        Service.Framework.Update += this.OnUpdate;
    }

    /// <summary>Target frames per second; 0 = no cap (the default).</summary>
    internal int TargetFps { get; set; }

    private void OnUpdate(IFramework framework)
    {
        if (this.TargetFps > 0)
        {
            var sleep = FramePacer.SleepMs(this.TargetFps, this.sinceLastFrame.Elapsed.TotalMilliseconds);
            if (sleep > 0)
                Thread.Sleep(sleep);
        }

        this.sinceLastFrame.Restart();
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        this.TargetFps = 0;
    }
}
