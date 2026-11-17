using System;
using Dalamud.Interface.Windowing;

namespace TextBasedRPG.Windows;

/// <summary>Owns the plugin's windows and hooks them into Dalamud's UI builder (SamplePlugin pattern).</summary>
internal sealed class PluginUi : IDisposable
{
    private readonly WindowSystem windows = new("TextBasedRPG");

    internal PluginUi(Plugin plugin)
    {
        this.Settings = new SettingsWindow(plugin);
        this.windows.AddWindow(this.Settings);

        Service.PluginInterface.UiBuilder.Draw += this.windows.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleSettings;
        Service.PluginInterface.UiBuilder.OpenMainUi += this.ToggleSettings;
    }

    internal SettingsWindow Settings { get; }

    /// <summary>Opens or closes the settings window (plugin installer buttons and <c>/trpg settings</c>).</summary>
    internal void ToggleSettings() => this.Settings.Toggle();

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= this.windows.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleSettings;
        Service.PluginInterface.UiBuilder.OpenMainUi -= this.ToggleSettings;
        this.windows.RemoveAllWindows();
    }
}
