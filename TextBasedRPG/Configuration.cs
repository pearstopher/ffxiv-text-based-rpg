using System;
using Dalamud.Configuration;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// The plugin's saved settings, stored by Dalamud in <c>pluginConfigs\TextBasedRPG.json</c>.
/// </summary>
/// <remarks>Every public settable property here must be documented in docs/SETTINGS.md.</remarks>
[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    /// <summary>Frame-rate cap while the mode is on and you're not typing.</summary>
    public int IdleFps { get; set; } = FpsTarget.DefaultIdleFps;

    /// <summary>Frame-rate cap while typing in chat (and 3 s after).</summary>
    public int TypingFps { get; set; } = FpsTarget.DefaultTypingFps;

    /// <summary>Show the TRPG entry in the server info bar.</summary>
    public bool ShowDtrEntry { get; set; } = true;

    /// <summary>Turn the mode on while the game marks you AFK, and off again when you're back (off by default).</summary>
    public bool AutoOnWhenAfk { get; set; }

    /// <summary>Only switch the 3D world off; keep the HUD and the frame rate as they are (off by default: full text-only mode).</summary>
    public bool WorldOffOnly { get; set; }

    /// <summary>Dev builds only: run the in-game self-tests once after each load, when a character is logged in.</summary>
    public bool DevRunSelfTestsOnLoad { get; set; } = true;

    /// <summary>Loads the saved settings with Dalamud's config API, or returns defaults. Out-of-range values are fixed.</summary>
    internal static Configuration Load()
    {
        var config = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        config.Clamp();
        return config;
    }

    /// <summary>Puts every value back into its allowed range.</summary>
    internal void Clamp()
    {
        this.IdleFps = SettingLimits.ClampIdleFps(this.IdleFps);
        this.TypingFps = SettingLimits.ClampTypingFps(this.TypingFps);
    }

    /// <summary>Saves the settings with Dalamud's config API.</summary>
    internal void Save() => Service.PluginInterface.SavePluginConfig(this);
}
