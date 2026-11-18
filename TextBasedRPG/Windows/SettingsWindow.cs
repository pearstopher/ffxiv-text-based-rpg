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

        if (ImGui.Button(mode.IsOn ? "Leave text-only mode" : "Enter text-only mode"))
        {
            // Draw isn't guaranteed to run on the framework thread; the switch touches game UI memory.
            Service.Framework.RunOnFrameworkThread(() => mode.RequestAndReport(ModeRequest.Toggle));
        }

        ImGui.TextWrapped($"Or type {Commands.Alias}. In the mode only the chat window stays, the 3D world and the rest of the HUD are hidden, and the frame rate is capped.");
        ImGui.Separator();

        var changed = false;
        var worldOnly = config.WorldOffOnly;
        if (ImGui.Checkbox("3D world off only (HUD and frame rate unchanged)", ref worldOnly))
        {
            config.WorldOffOnly = worldOnly;
            changed = true;
        }

        ImGui.TextDisabled("Unchecked (default) is full text-only mode. A change applies the next time you turn the mode on.");
        ImGui.Separator();

        // The frame rates only matter in full mode.
        using (ImRaii.Disabled(config.WorldOffOnly))
        {
            var idle = config.IdleFps;
            if (ImGui.SliderInt("Frame rate when idle", ref idle, SettingLimits.IdleFpsMin, SettingLimits.IdleFpsMax))
            {
                config.IdleFps = idle;
                changed = true;
            }

            var typing = config.TypingFps;
            if (ImGui.SliderInt("Frame rate while typing", ref typing, SettingLimits.TypingFpsMin, SettingLimits.TypingFpsMax))
            {
                config.TypingFps = typing;
                changed = true;
            }

            ImGui.TextDisabled("Lower frame rates save more CPU. Typing uses the higher one (and for 3 s after).");
        }

        var showDtr = config.ShowDtrEntry;
        if (ImGui.Checkbox("Show TRPG in the server info bar", ref showDtr))
        {
            config.ShowDtrEntry = showDtr;
            changed = true;
        }

        var autoAfk = config.AutoOnWhenAfk;
        if (ImGui.Checkbox("Turn on when I go AFK", ref autoAfk))
        {
            config.AutoOnWhenAfk = autoAfk;
            changed = true;
        }

        ImGui.TextDisabled("When the game marks you AFK, the mode turns on, and off again when you're back. If it was already on, it stays on.");

        if (changed)
        {
            // Applying touches framework-thread state (frame limiter, server info bar, AFK watcher); Draw may not be on it.
            Service.Framework.RunOnFrameworkThread(this.plugin.ApplySettings);
            config.Save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted(this.plugin.Cpu.Describe());
        ImGui.TextUnformatted(this.plugin.Cpu.Saving ?? "Use the mode for a few seconds to see how much CPU it saves.");

        this.DrawSelfTests();
    }

    private void DrawSelfTests()
    {
        ImGui.Separator();
        var runner = this.plugin.SelfTests;
        using (ImRaii.Disabled(runner.IsRunning))
        {
            if (ImGui.Button(runner.IsRunning ? "Tests running..." : "Run tests"))
                Service.Framework.RunOnFrameworkThread(() => runner.Start());
        }

        ImGui.TextWrapped($"Checks the mode works on your game and measures how much CPU and GPU it saves; the results appear in chat. Takes about 1½ minutes.{SelfTestRunner.StartNote}");
    }
}
