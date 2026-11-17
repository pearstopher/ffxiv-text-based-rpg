using System.Text.Json;

namespace TextBasedRPG.Core;

/// <summary>One loaded UI addon, as seen by <c>/trpgdebug addons</c> and the HUD hider's checks.</summary>
/// <param name="Name">Addon name, e.g. <c>ChatLog</c> or <c>_ActionBar</c>.</param>
/// <param name="IsVisible">The game's own visibility state for the addon.</param>
/// <param name="RootNodeVisible">Whether the addon's root node is drawn (what the HUD hider toggles).</param>
/// <param name="DepthLayer">The addon's depth layer.</param>
public sealed record AddonInfo(string Name, bool IsVisible, bool RootNodeVisible, uint DepthLayer)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Content of <c>addons.json</c>: the addons sorted by name, with when and which build wrote it.</summary>
    public static string ToJson(IEnumerable<AddonInfo> addons, DateTimeOffset at, string build)
        => JsonSerializer.Serialize(
            new { WrittenAt = at, Build = build, Addons = addons.OrderBy(a => a.Name, StringComparer.Ordinal).ToList() },
            JsonOptions);
}
