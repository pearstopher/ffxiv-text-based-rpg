using System;
using Dalamud.Plugin;
using TextBasedRPG.Core;

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

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        this.Config = Configuration.Load();

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
