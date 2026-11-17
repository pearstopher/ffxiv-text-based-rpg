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
/// <remarks>
/// While <see cref="Active"/>, the cap is <see cref="IdleFps"/>, or <see cref="TypingFps"/> while the chat input is
/// active and for 3 s after (<see cref="FpsTarget"/>), so typing stays responsive.
/// </remarks>
internal sealed class FrameLimiter : IDisposable
{
    private readonly Stopwatch sinceLastFrame = Stopwatch.StartNew();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly FpsTarget target = new();
    private bool active;

    internal FrameLimiter()
    {
        Service.Framework.Update += this.OnUpdate;
    }

    /// <summary>Whether the cap is applied. Off by default.</summary>
    internal bool Active
    {
        get => this.active;
        set
        {
            if (value && !this.active)
                this.target.Reset();
            this.active = value;
            if (!value)
                this.TargetFps = 0;
        }
    }

    /// <summary>Cap while idle.</summary>
    internal int IdleFps { get; set; } = FpsTarget.DefaultIdleFps;

    /// <summary>Cap while typing.</summary>
    internal int TypingFps { get; set; } = FpsTarget.DefaultTypingFps;

    /// <summary>The cap used for the last frame; 0 = none.</summary>
    internal int TargetFps { get; private set; }

    private void OnUpdate(IFramework framework)
    {
        if (this.active)
        {
            this.TargetFps = this.target.Choose(this.IdleFps, this.TypingFps, ChatInput.IsTyping, this.clock.ElapsedMilliseconds);
            var sleep = FramePacer.SleepMs(this.TargetFps, this.sinceLastFrame.Elapsed.TotalMilliseconds);
            if (sleep > 0)
                Thread.Sleep(sleep);
        }

        this.sinceLastFrame.Restart();
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        this.Active = false;
    }
}
