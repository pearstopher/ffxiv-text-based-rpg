using System;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Switches text-only mode: 3D off (<see cref="RenderController"/>), frame-rate cap (<see cref="FrameLimiter"/>)
/// and HUD hidden except the chat (<see cref="HudHider"/>), all together. <see cref="ModeState"/> (Core) decides
/// whether a request is allowed.
/// </summary>
internal sealed class ModeController : IDisposable
{
    private readonly ModeState state = new();
    private readonly RenderController render;
    private readonly FrameLimiter limiter;
    private readonly HudHider hud;
    private readonly CpuMonitor cpu;
    private ModeParts active = ModeParts.Full;

    internal ModeController(RenderController render, FrameLimiter limiter, HudHider hud, CpuMonitor cpu)
    {
        this.render = render;
        this.limiter = limiter;
        this.hud = hud;
        this.cpu = cpu;
        Service.ClientState.Logout += this.OnLogout;
    }

    /// <summary>True while the mode is on.</summary>
    internal bool IsOn => this.state.IsOn;

    /// <summary>How many times the mode has been switched on or off (lets a part tell whether anyone else switched it).</summary>
    internal int Changes { get; private set; }

    /// <summary>The "3D world off only" setting; read when the mode is turned on, so a change applies the next time.</summary>
    internal bool WorldOffOnly { get; set; }

    /// <summary>The parts applied by the current (or last) switch on.</summary>
    internal ModeParts Parts => this.active;

    /// <summary>
    /// Applies a request (toggle/on/off) and returns what happened. Framework thread only.
    /// <paramref name="parts"/> overrides the setting (self-tests always run the full mode).
    /// </summary>
    internal ModeChange Request(ModeRequest request, ModeParts? parts = null)
        => this.Apply(this.state.Request(request, Service.ClientState.IsLoggedIn), parts);

    /// <summary>Applies a request and tells the player in chat what happened (commands, DTR entry, settings window).</summary>
    internal void RequestAndReport(ModeRequest request)
    {
        var (text, isError) = ModeMessages.For(this.Request(request), this.IsOn, this.active);
        if (isError)
            Service.ChatGui.PrintError(text);
        else
            Service.ChatGui.Print(text);
    }

    /// <summary>Turns the mode off because the character logged out.</summary>
    internal ModeChange LoggedOut() => this.Apply(this.state.LoggedOut());

    private void OnLogout(int type, int code)
    {
        if (this.LoggedOut() == ModeChange.TurnedOff)
            Service.Log.Information("Text-only mode turned off because the character logged out");
    }

    /// <summary>Unload: stop listening and turn the mode off, so nothing stays hidden or capped.</summary>
    public void Dispose()
    {
        Service.ClientState.Logout -= this.OnLogout;
        this.Apply(this.state.Request(ModeRequest.Off, loggedIn: false));
    }

    private ModeChange Apply(ModeChange change, ModeParts? parts = null)
    {
        switch (change)
        {
            case ModeChange.TurnedOn:
                // The parts are remembered, so mode off undoes exactly what was applied.
                this.active = parts ?? ModeParts.For(this.WorldOffOnly);
                this.render.Want3DOff = true;
                this.limiter.Active = this.active.FrameCap;
                this.hud.Active = this.active.HideHud;
                break;

            case ModeChange.TurnedOff:
                // Reverse order: HUD back first, then the frame rate, then the 3D world.
                this.hud.Active = false;
                this.limiter.Active = false;
                this.render.Want3DOff = false;
                break;

            default:
                return change;
        }

        this.Changes++;
        this.cpu.ResetAverage();
        this.cpu.SetMode(this.IsOn);
        Service.Log.Information($"Text-only mode {(this.IsOn ? "on" : "off")}");
        return change;
    }
}
