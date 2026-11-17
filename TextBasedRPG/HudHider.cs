using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
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
/// <para>
/// Some addons (e.g. job gauges like <c>JobHudBRD0</c>) turn their own root node back on in their update, so between
/// scans they'd be drawn. When a scan finds an addon it already hid drawn again, it adds a Dalamud
/// <see cref="IAddonLifecycle"/> PreDraw listener for that addon name that hides it again just before it's drawn.
/// Only those addons get per-frame work, and only while active; the listeners are removed when the mode turns off.
/// </para>
/// </remarks>
internal sealed unsafe class HudHider : IDisposable
{
    /// <summary>How often to look for newly drawn addons while active.</summary>
    internal const long RescanMs = 250;

    private readonly Dictionary<nint, string> hidden = [];
    private readonly HashSet<string> guarded = [];
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
            {
                this.Scan();
            }
            else
            {
                this.StopGuarding();
                this.RestoreAll();
            }
        }
    }

    /// <summary>Number of addons currently hidden by this class.</summary>
    internal int HiddenCount => this.hidden.Count;

    /// <summary>Addons that showed themselves again and are now re-hidden before every draw.</summary>
    internal IReadOnlyCollection<string> Guarded => this.guarded;

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

            // Hidden before and drawn again: the addon shows itself, so hide it before every draw from now on.
            if (this.hidden.TryGetValue((nint)addon, out var hiddenName) && hiddenName == name)
                this.Guard(name);

            addon->RootNode->ToggleVisibility(false);
            this.hidden[(nint)addon] = name;
        }
    }

    private void Guard(string name)
    {
        if (!this.guarded.Add(name))
            return;
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, name, this.OnPreDraw);
        Service.Log.Debug($"HUD hider: {name} shows itself again; hiding it before every draw while the mode is on.");
    }

    private void StopGuarding()
    {
        if (this.guarded.Count == 0)
            return;
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, this.guarded, this.OnPreDraw);
        this.guarded.Clear();
    }

    private void OnPreDraw(AddonEvent type, AddonArgs args)
    {
        if (!this.active)
            return;

        var addon = (AtkUnitBase*)args.Addon.Address;
        if (addon == null || addon->RootNode == null || !addon->RootNode->IsVisible())
            return;

        addon->RootNode->ToggleVisibility(false);
        this.hidden[(nint)addon] = args.AddonName;
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
