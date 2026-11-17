using System;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Switches text-based RPG mode: 3D off (<see cref="RenderController"/>), frame-rate cap (<see cref="FrameLimiter"/>)
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

    /// <summary>Applies a request (toggle/on/off) and returns what happened. Framework thread only.</summary>
    internal ModeChange Request(ModeRequest request)
        => this.Apply(this.state.Request(request, Service.ClientState.IsLoggedIn));

    /// <summary>Applies a request and tells the player in chat what happened (commands, DTR entry, settings window).</summary>
    internal void RequestAndReport(ModeRequest request)
    {
        var (text, isError) = ModeMessages.For(this.Request(request), this.IsOn);
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
            Service.Log.Information("Text-based RPG mode turned off because the character logged out");
    }

    /// <summary>Unload: stop listening and turn the mode off, so nothing stays hidden or capped.</summary>
    public void Dispose()
    {
        Service.ClientState.Logout -= this.OnLogout;
        this.Apply(this.state.Request(ModeRequest.Off, loggedIn: false));
    }

    private ModeChange Apply(ModeChange change)
    {
        switch (change)
        {
            case ModeChange.TurnedOn:
                this.render.Want3DOff = true;
                this.limiter.Active = true;
                this.hud.Active = true;
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

        this.cpu.ResetAverage();
        this.cpu.SetMode(this.IsOn);
        Service.Log.Information($"Text-based RPG mode {(this.IsOn ? "on" : "off")}");
        return change;
    }
}
