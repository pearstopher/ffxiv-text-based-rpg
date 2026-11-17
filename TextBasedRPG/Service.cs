using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace TextBasedRPG;

/// <summary>
/// The Dalamud services the plugin uses, injected once by Dalamud (<see cref="IDalamudPluginInterface.Create{T}"/>).
/// </summary>
/// <remarks>
/// Same pattern as SamplePlugin and Simple Tweaks: static properties marked <see cref="PluginServiceAttribute"/>.
/// Add a service here only when a feature needs it.
/// </remarks>
internal sealed class Service
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] internal static ISelfTestRegistry SelfTestRegistry { get; private set; } = null!;

    /// <summary>Has Dalamud fill in the static service properties.</summary>
    internal static void Initialize(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<Service>();
}
