using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Entry point. Creates the plugin's services and releases them all in <see cref="Dispose"/>.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

#if DEBUG
    internal const string BuildConfiguration = "Debug";
#else
    internal const string BuildConfiguration = "Release";
#endif

    public Plugin()
    {
        // Logs which build is running, so a stale or wrong-configuration build is obvious in /xllog.
        Log.Information($"{PluginInterface.Manifest.Name} loaded ({DescribeBuild()})");
    }

    /// <summary>E.g. "Debug build, built 2026-11-16 10:36:02, from C:\...\TextBasedRPG.dll".</summary>
    internal static string DescribeBuild()
    {
        var dll = PluginInterface.AssemblyLocation;
        return BuildInfo.Describe(BuildConfiguration, BuildInfo.ReadBuildTime(dll.FullName), dll.FullName);
    }

    public void Dispose()
    {
        Log.Information($"{PluginInterface.Manifest.Name} unloaded");
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
            Log.Error(ex, $"Failed to dispose {name}");
        }
    }
}
