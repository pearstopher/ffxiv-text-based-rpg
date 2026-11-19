using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using TextBasedRPG.Core;
using TextBasedRPG.SelfTests;

namespace TextBasedRPG.Windows;

/// <summary>The settings window (SamplePlugin's ConfigWindow pattern). Settings save as soon as they change.</summary>
internal sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;

    internal SettingsWindow(Plugin plugin)
        : base("Text-Only Mode###TextBasedRPGSettings")
    {
        this.plugin = plugin;
        this.Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;
        this.SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(320, 0) };
    }

    public override void Draw()
    {
        var config = this.plugin.Config;
        var mode = this.plugin.Mode;

        if (ImGui.Button(Strings.Get(mode.IsOn ? "settings.leave" : "settings.enter") + "###toggle"))
        {
            // Draw isn't guaranteed to run on the framework thread; the switch touches game UI memory.
            Service.Framework.RunOnFrameworkThread(() => mode.RequestAndReport(ModeRequest.Toggle));
        }

        ImGui.TextWrapped(Strings.Get("settings.intro", Commands.Main));
        ImGui.Separator();

        var changed = false;
        var worldOnly = config.WorldOffOnly;
        if (ImGui.Checkbox(Strings.Get("settings.worldOnly") + "###worldOnly", ref worldOnly))
        {
            config.WorldOffOnly = worldOnly;
            changed = true;
        }

        ImGui.TextDisabled(Strings.Get("settings.worldOnly.help"));
        ImGui.Separator();

        // The frame rates only matter in full mode.
        using (ImRaii.Disabled(config.WorldOffOnly))
        {
            var idle = config.IdleFps;
            if (ImGui.SliderInt(Strings.Get("settings.idleFps") + "###idleFps", ref idle, SettingLimits.IdleFpsMin, SettingLimits.IdleFpsMax))
            {
                config.IdleFps = idle;
                changed = true;
            }

            var typing = config.TypingFps;
            if (ImGui.SliderInt(Strings.Get("settings.typingFps") + "###typingFps", ref typing, SettingLimits.TypingFpsMin, SettingLimits.TypingFpsMax))
            {
                config.TypingFps = typing;
                changed = true;
            }

            ImGui.TextDisabled(Strings.Get("settings.fps.help"));
        }

        var showDtr = config.ShowDtrEntry;
        if (ImGui.Checkbox(Strings.Get("settings.dtr") + "###dtr", ref showDtr))
        {
            config.ShowDtrEntry = showDtr;
            changed = true;
        }

        var autoAfk = config.AutoOnWhenAfk;
        if (ImGui.Checkbox(Strings.Get("settings.afk") + "###afk", ref autoAfk))
        {
            config.AutoOnWhenAfk = autoAfk;
            changed = true;
        }

        ImGui.TextDisabled(Strings.Get("settings.afk.help"));

        if (changed)
        {
            // Applying touches framework-thread state (frame limiter, server info bar, AFK watcher); Draw may not be on it.
            Service.Framework.RunOnFrameworkThread(this.plugin.ApplySettings);
            config.Save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted(this.plugin.Cpu.Describe());
        ImGui.TextUnformatted(this.plugin.Cpu.Saving ?? Strings.Get("settings.saving.none"));

        this.DrawSelfTests();
    }

    private void DrawSelfTests()
    {
        ImGui.Separator();
        var runner = this.plugin.SelfTests;
        using (ImRaii.Disabled(runner.IsRunning))
        {
            if (ImGui.Button(Strings.Get(runner.IsRunning ? "settings.tests.running" : "settings.tests.run") + "###runTests"))
                Service.Framework.RunOnFrameworkThread(() => runner.Start());
        }

        ImGui.TextWrapped(Strings.Get("settings.tests.help", SelfTestRunner.StartNote));
    }
}
