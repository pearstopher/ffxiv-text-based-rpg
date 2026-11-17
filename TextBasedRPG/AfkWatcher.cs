using System;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// The "Turn on when AFK" setting: turns the mode on when the game marks the player AFK (online status "Away from
/// Keyboard", read with Dalamud's <see cref="IObjectTable.LocalPlayer"/>). It never turns the mode off: chatting can
/// clear AFK, and the mode is for chatting. <see cref="AfkTrigger"/> (Core) decides when.
/// </summary>
/// <remarks>Only subscribed to the framework while the setting is on, and then checks once a second.</remarks>
internal sealed class AfkWatcher : IDisposable
{
    private const long CheckIntervalMs = 1_000;

    private readonly ModeController mode;
    private readonly AfkTrigger trigger = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private long nextCheckMs;
    private bool enabled;

    internal AfkWatcher(ModeController mode) => this.mode = mode;

    /// <summary>Whether the setting is on.</summary>
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
    private static bool? IsAfk
        => Service.ObjectTable.LocalPlayer is { } player ? player.OnlineStatus.RowId == AfkTrigger.AfkOnlineStatus : null;

    private void OnUpdate(IFramework framework)
    {
        var now = this.clock.ElapsedMilliseconds;
        if (now < this.nextCheckMs)
            return;
        this.nextCheckMs = now + CheckIntervalMs;

        if (!this.trigger.Update(IsAfk) || this.mode.IsOn)
            return;

        if (this.mode.Request(ModeRequest.On) == ModeChange.TurnedOn)
        {
            Service.Log.Information("Text-only mode turned on because the character went AFK");
            Service.ChatGui.Print(ModeMessages.AutoOnAfk);
        }
    }

    public void Dispose() => this.Enabled = false;
}
