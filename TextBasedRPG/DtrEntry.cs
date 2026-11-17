using System;
using Dalamud.Game.Gui.Dtr;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// The plugin's entry in the server info bar (Dalamud's <c>IDtrBar</c>). Clicking it turns the mode on. While the
/// mode is on, the server info bar is hidden with the rest of the HUD, so the mode is left with <c>/trpg</c>.
/// </summary>
internal sealed class DtrEntry : IDisposable
{
    private const string Title = "Text-Only Mode";

    private readonly Plugin plugin;
    private readonly IDtrBarEntry entry;

    internal DtrEntry(Plugin plugin)
    {
        this.plugin = plugin;
        this.entry = Service.DtrBar.Get(Title);
        this.entry.Text = "TRPG";
        this.entry.Tooltip = $"Text-Only Mode: click to hide everything but the chat and save CPU.\nType {Commands.Alias} to come back.";
        this.entry.OnClick = this.OnClick;
    }

    /// <summary>Whether the entry is shown in the server info bar.</summary>
    internal bool Shown
    {
        get => this.entry.Shown;
        set => this.entry.Shown = value;
    }

    // Server info bar clicks are handled by the game's UI on the framework thread.
    private void OnClick(DtrInteractionEvent e) => this.plugin.Mode.RequestAndReport(ModeRequest.Toggle);

    public void Dispose() => this.entry.Remove();
}
