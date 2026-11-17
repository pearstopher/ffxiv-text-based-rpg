using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using TextBasedRPG.Core;

namespace TextBasedRPG.Windows;

/// <summary>The settings window (SamplePlugin's ConfigWindow pattern). Settings save as soon as they change.</summary>
internal sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;

    internal SettingsWindow(Plugin plugin)
        : base("Text-Based RPG###TextBasedRPGSettings")
    {
        this.plugin = plugin;
        this.Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;
        this.SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(320, 0) };
    }

    public override void Draw()
    {
        var config = this.plugin.Config;
        var mode = this.plugin.Mode;

        if (ImGui.Button(mode.IsOn ? "Leave text-based RPG mode" : "Enter text-based RPG mode"))
        {
            var change = mode.Request(ModeRequest.Toggle);
            var (text, isError) = ModeMessages.For(change, mode.IsOn);
            if (isError)
                Service.ChatGui.PrintError(text);
            else
                Service.ChatGui.Print(text);
        }

        ImGui.TextWrapped($"Or type {Commands.Alias}. In the mode only the chat window stays, the 3D world and the rest of the HUD are hidden, and the frame rate is capped.");
        ImGui.Separator();

        var changed = false;
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

        var showDtr = config.ShowDtrEntry;
        if (ImGui.Checkbox("Show TRPG in the server info bar", ref showDtr))
        {
            config.ShowDtrEntry = showDtr;
            changed = true;
        }

        if (changed)
        {
            this.plugin.ApplySettings();
            config.Save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted(this.plugin.Cpu.Describe());
    }
}
