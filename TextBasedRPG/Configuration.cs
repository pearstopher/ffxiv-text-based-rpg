using System;
using Dalamud.Configuration;

namespace TextBasedRPG;

/// <summary>
/// The plugin's saved settings, stored by Dalamud in <c>pluginConfigs\TextBasedRPG.json</c>.
/// </summary>
/// <remarks>Every public settable property here must be documented in docs/SETTINGS.md.</remarks>
[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    /// <summary>Dev builds only: run the in-game self-tests once after each load, when a character is logged in.</summary>
    public bool DevRunSelfTestsOnLoad { get; set; } = true;

    /// <summary>Loads the saved settings with Dalamud's config API, or returns defaults.</summary>
    internal static Configuration Load()
        => Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    /// <summary>Saves the settings with Dalamud's config API.</summary>
    internal void Save() => Service.PluginInterface.SavePluginConfig(this);
}
