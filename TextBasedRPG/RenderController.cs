using System;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Turns 3D rendering off and on for the mode. Callers set <see cref="Want3DOff"/>; each frame the change is applied
/// through <see cref="RenderDisableManager"/> as soon as <see cref="TransitionGuard"/> says it's safe (never during a
/// zone load or cutscene, or within 3 s after one). Until then the current state is held.
/// </summary>
/// <remarks>Per frame: five condition reads and a compare.</remarks>
internal sealed class RenderController : IDisposable
{
    private readonly TransitionGuard guard = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();

    internal RenderController()
    {
        Service.Framework.Update += this.OnUpdate;
    }

    /// <summary>What the mode wants: true = 3D off.</summary>
    internal bool Want3DOff { get; set; }

    /// <summary>True once our 3D-off request is in place.</summary>
    internal bool IsApplied { get; private set; }

    /// <summary>True while a wanted change waits for a zone load or cutscene to finish.</summary>
    internal bool IsWaiting => this.Want3DOff != this.IsApplied;

    private void OnUpdate(IFramework framework)
    {
        var canFlip = this.guard.Update(GameState.InTransition, this.clock.ElapsedMilliseconds);
        if (!this.IsWaiting || !canFlip)
            return;

        if (this.Want3DOff)
            RenderDisableManager.PlaceRequest();
        else
            RenderDisableManager.RemoveRequest();

        this.IsApplied = this.Want3DOff;
        Service.Log.Debug($"3D rendering {(this.IsApplied ? "off" : "on")} request applied");
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        if (this.IsApplied && Service.Framework.IsInFrameworkUpdateThread)
            RenderDisableManager.RemoveRequest();
        this.IsApplied = false;

        // RenderDisableManager.Dispose (called by Plugin afterwards) also removes the request and applies it.
    }
}
