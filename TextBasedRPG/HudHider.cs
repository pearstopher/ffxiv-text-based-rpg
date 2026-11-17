using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// While <see cref="Active"/>, hides every drawn UI addon except the chat by turning off its root node, the way
/// Simple Tweaks' "Hide Chat" tweak hides the chat (<c>RootNode-&gt;ToggleVisibility</c>). Only drawing is affected:
/// the game's own show/hide state is untouched. Turning it off shows exactly the nodes it hid.
/// </summary>
/// <remarks>
/// Re-scans every <see cref="RescanMs"/> ms while active, to catch addons the game opens or re-creates (zone changes).
/// Hidden addons are remembered by address and name; before showing one again it must still be loaded, with the
/// same name (addresses can be reused). Framework thread only.
/// </remarks>
internal sealed unsafe class HudHider : IDisposable
{
    /// <summary>How often to look for newly drawn addons while active.</summary>
    internal const long RescanMs = 250;

    private readonly Dictionary<nint, string> hidden = [];
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private long nextScanMs;
    private bool active;

    internal HudHider()
    {
        Service.Framework.Update += this.OnUpdate;
    }

    /// <summary>Whether the HUD is hidden. Setting false shows everything this class hid.</summary>
    internal bool Active
    {
        get => this.active;
        set
        {
            if (value == this.active)
                return;

            this.active = value;
            if (value)
                this.Scan();
            else
                this.RestoreAll();
        }
    }

    /// <summary>Number of addons currently hidden by this class.</summary>
    internal int HiddenCount => this.hidden.Count;

    private void OnUpdate(IFramework framework)
    {
        if (!this.active)
            return;

        var now = this.clock.ElapsedMilliseconds;
        if (now < this.nextScanMs)
            return;

        this.nextScanMs = now + RescanMs;
        this.Scan();
    }

    /// <summary>Hides the root node of every drawn addon the policy says to hide.</summary>
    private void Scan()
    {
        var list = UiAddons.AllLoaded;
        if (list == null)
            return;

        foreach (var entry in list->Entries[..list->Count])
        {
            var addon = entry.Value;
            if (addon == null || addon->RootNode == null || !addon->RootNode->IsVisible())
                continue;

            var name = addon->NameString;
            if (!HudVisibilityPolicy.ShouldHide(name))
                continue;

            addon->RootNode->ToggleVisibility(false);
            this.hidden[(nint)addon] = name;
        }
    }

    /// <summary>Shows the root nodes this class hid, for addons that are still loaded under the same name.</summary>
    private void RestoreAll()
    {
        var list = UiAddons.AllLoaded;
        if (list != null && this.hidden.Count > 0)
        {
            foreach (var entry in list->Entries[..list->Count])
            {
                var addon = entry.Value;
                if (addon == null || addon->RootNode == null)
                    continue;
                if (!this.hidden.TryGetValue((nint)addon, out var name) || addon->NameString != name)
                    continue;
                if (!addon->RootNode->IsVisible())
                    addon->RootNode->ToggleVisibility(true);
            }
        }

        this.hidden.Clear();
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        this.Active = false;
    }
}
