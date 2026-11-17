using System;
using Dalamud.Plugin;
using TextBasedRPG.Core;
using TextBasedRPG.SelfTests;

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

    private readonly SelfTestTriggers selfTestTriggers;
    private readonly PluginCommands commands;
    private readonly DebugCommands debugCommands;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        this.Config = Configuration.Load();
        this.Cpu = new CpuMonitor();
        this.Render = new RenderController();
        this.Limiter = new FrameLimiter();

        // In-game checks: Dalamud's Self-Test window gets its own step instances; the dev runner builds fresh ones per run.
        // Dalamud unregisters them when the plugin unloads.
        Service.SelfTestRegistry.RegisterTestSteps(SelfTestCatalog.Create());
        this.SelfTests = new SelfTestRunner();
        this.selfTestTriggers = new SelfTestTriggers(this.SelfTests, this.Config);
        this.commands = new PluginCommands();
        this.debugCommands = new DebugCommands(this);

        // Logs which build is running, so a stale or wrong-configuration build is obvious in /xllog.
        Service.Log.Information($"{Service.PluginInterface.Manifest.Name} loaded ({DescribeBuild()})");
    }

    /// <summary>E.g. "Debug build, built 2026-11-16 10:36:02, from C:\...\TextBasedRPG.dll".</summary>
    internal static string DescribeBuild()
    {
        var dll = Service.PluginInterface.AssemblyLocation;
        return BuildInfo.Describe(BuildConfiguration, BuildInfo.ReadBuildTime(dll.FullName), dll.FullName);
    }

    public void Dispose()
    {
        DisposeSafely(this.debugCommands, "debug commands");
        DisposeSafely(this.commands, "commands");
        DisposeSafely(this.selfTestTriggers, "self-test triggers");
        DisposeSafely(this.SelfTests, "self-test runner");
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
