using System;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// The "Turn on when AFK" setting: while the game marks the player AFK (online status "Away from Keyboard", read with
/// Dalamud's <see cref="IObjectTable.LocalPlayer"/>), the mode is on. If the setting turned it on, it turns off again
/// when the player is back; a mode the player turned on themselves is left alone. <see cref="AfkTrigger"/> (Core)
/// decides.
/// </summary>
/// <remarks>
/// Only subscribed to the framework while the setting is on, and then checks once a second. It doesn't switch the
/// mode during a test run (that would spoil the measurements). Any mode change it didn't make (the player, a logout,
/// a test run) means the mode is no longer its to turn off: <see cref="ModeController.Changes"/> tells.
/// </remarks>
internal sealed class AfkWatcher : IDisposable
{
    private readonly ModeController mode;
    private readonly Func<bool> paused;
    private readonly Func<bool?> isAfk;
    private readonly long checkIntervalMs;
    private readonly AfkTrigger trigger = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private long nextCheckMs;
    private int changesAfterOurs;
    private bool enabled;

    /// <param name="mode">The mode to switch.</param>
    /// <param name="paused">True while the watcher must not switch the mode (a test run is measuring).</param>
    /// <param name="isAfk">The AFK state; defaults to the local player's online status. The afk-auto self-test passes a simulated one.</param>
    /// <param name="checkIntervalMs">How often to check (default once a second; the self-test checks every frame).</param>
    internal AfkWatcher(ModeController mode, Func<bool> paused, Func<bool?>? isAfk = null, long checkIntervalMs = 1_000)
    {
        this.mode = mode;
        this.paused = paused;
        this.isAfk = isAfk ?? (() => PlayerIsAfk);
        this.checkIntervalMs = checkIntervalMs;
    }

    /// <summary>Whether the setting is on. Turning it off leaves the mode as it is.</summary>
    internal bool Enabled
    {
        get => this.enabled;
        set
        {
            if (value == this.enabled)
                return;

            this.enabled = value;
            this.trigger.Reset();
            if (value)
                Service.Framework.Update += this.OnUpdate;
            else
                Service.Framework.Update -= this.OnUpdate;
        }
    }

    /// <summary>True when the local player has the AFK online status; null without a character.</summary>
    internal static bool? PlayerIsAfk
        => Service.ObjectTable.LocalPlayer is { } player ? player.OnlineStatus.RowId == AfkTrigger.AfkOnlineStatus : null;

    private void OnUpdate(IFramework framework)
    {
        var now = this.clock.ElapsedMilliseconds;
        if (now < this.nextCheckMs)
            return;
        this.nextCheckMs = now + this.checkIntervalMs;

        if (this.trigger.TurnedOnByAfk && this.mode.Changes != this.changesAfterOurs)
            this.trigger.ModeChangedByPlayer();

        var action = this.trigger.Update(this.isAfk(), this.mode.IsOn);
        if (action == AfkAction.None || this.paused())
            return;

        if (action == AfkAction.TurnOn && this.mode.Request(ModeRequest.On) == ModeChange.TurnedOn)
        {
            this.trigger.TurnedOn();
            this.changesAfterOurs = this.mode.Changes;
            Service.Log.Information("Text-only mode turned on because the character went AFK");
            Service.ChatGui.Print(ModeMessages.AutoOnAfk);
        }
        else if (action == AfkAction.TurnOff && this.mode.Request(ModeRequest.Off) == ModeChange.TurnedOff)
        {
            Service.Log.Information("Text-only mode turned off because the character is no longer AFK");
            Service.ChatGui.Print(ModeMessages.AutoOffAfk);
        }
    }

    public void Dispose() => this.Enabled = false;
}
