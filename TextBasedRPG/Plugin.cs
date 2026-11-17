using System;
using Dalamud.Plugin;
using TextBasedRPG.Core;
using TextBasedRPG.SelfTests;
using TextBasedRPG.Windows;

namespace TextBasedRPG;

/// <summary>
/// Entry point. Creates the plugin's services and releases them all in <see cref="Dispose"/>.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
#if DEBUG
    internal const string BuildConfiguration = "Debug";
#else
    internal const string BuildConfiguration = "Release";
#endif

    internal Configuration Config { get; }

    internal SelfTestRunner SelfTests { get; }

    internal CpuMonitor Cpu { get; }

    internal RenderController Render { get; }

    internal FrameLimiter Limiter { get; }

    internal HudHider Hud { get; }

    internal ModeController Mode { get; }

    private readonly AfkWatcher afkWatcher;
    private readonly SelfTestTriggers selfTestTriggers;
    private readonly PluginCommands commands;
    private readonly DebugCommands debugCommands;
    private readonly DtrEntry dtrEntry;

    internal PluginUi Ui { get; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        this.Config = Configuration.Load();
        this.Cpu = new CpuMonitor();
        this.Render = new RenderController();
        this.Limiter = new FrameLimiter();
        this.Hud = new HudHider();
        this.Mode = new ModeController(this.Render, this.Limiter, this.Hud, this.Cpu);
        this.afkWatcher = new AfkWatcher(this.Mode);

        // In-game checks: Dalamud's Self-Test window gets its own step instances; the dev runner builds fresh ones per run.
        // Dalamud unregisters them when the plugin unloads.
        Service.SelfTestRegistry.RegisterTestSteps(SelfTestCatalog.Create(this));
        this.SelfTests = new SelfTestRunner(this);
        this.selfTestTriggers = new SelfTestTriggers(this.SelfTests, this.Config);
        this.commands = new PluginCommands(this);
        this.debugCommands = new DebugCommands(this);
        this.dtrEntry = new DtrEntry(this);
        this.Ui = new PluginUi(this);
        this.ApplySettings();

        // Logs which build is running, so a stale or wrong-configuration build is obvious in /xllog.
        Service.Log.Information($"{Service.PluginInterface.Manifest.Name} loaded ({DescribeBuild()})");
    }

    /// <summary>Pushes the saved settings to the parts that use them (after load, and after the settings window changes them).</summary>
    internal void ApplySettings()
    {
        this.Config.Clamp();
        this.Limiter.IdleFps = this.Config.IdleFps;
        this.Limiter.TypingFps = this.Config.TypingFps;
        this.dtrEntry.Shown = this.Config.ShowDtrEntry;
        this.afkWatcher.Enabled = this.Config.AutoOnWhenAfk;
    }

    /// <summary>The <c>/trpg status</c> line: mode, 3D, FPS cap, hidden UI elements, CPU and FPS.</summary>
    internal string StatusText()
        => ModeMessages.Status(
            this.Mode.IsOn,
            this.Render.IsApplied,
            this.Render.IsWaiting,
            this.Limiter.TargetFps,
            this.Hud.HiddenCount,
            this.Cpu.Describe());

    /// <summary>E.g. "Debug build, built 2026-11-16 10:36:02, from C:\...\TextBasedRPG.dll".</summary>
    internal static string DescribeBuild()
    {
        var dll = Service.PluginInterface.AssemblyLocation;
        return BuildInfo.Describe(BuildConfiguration, BuildInfo.ReadBuildTime(dll.FullName), dll.FullName);
    }

    public void Dispose()
    {
        DisposeSafely(this.Ui, "windows");
        DisposeSafely(this.dtrEntry, "server info bar entry");
        DisposeSafely(this.debugCommands, "debug commands");
        DisposeSafely(this.commands, "commands");
        DisposeSafely(this.selfTestTriggers, "self-test triggers");
        DisposeSafely(this.SelfTests, "self-test runner");
        DisposeSafely(this.afkWatcher, "AFK watcher");
        DisposeSafely(this.Mode, "mode"); // turns the mode off first
        DisposeSafely(this.Hud, "HUD hider");
        DisposeSafely(this.Limiter, "frame limiter");
        DisposeSafely(this.Render, "render controller");
        try
        {
            RenderDisableManager.Dispose(); // removes our 3D-off request, if any
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Failed to dispose the render disable manager");
        }

        DisposeSafely(this.Cpu, "CPU monitor");
        Service.Log.Information($"{Service.PluginInterface.Manifest.Name} unloaded");
    }

    /// <summary>Disposes one part, logging instead of throwing, so the remaining parts are still released.</summary>
    internal static void DisposeSafely(IDisposable? part, string name)
    {
        if (part == null)
            return;

        try
        {
            part.Dispose();
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, $"Failed to dispose {name}");
        }
    }
}
