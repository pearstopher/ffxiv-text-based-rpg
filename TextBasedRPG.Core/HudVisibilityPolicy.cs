namespace TextBasedRPG.Core;

/// <summary>
/// Which UI addons stay visible in text-only mode: only the chat (owner decision; no pop-ups, prompts or other
/// HUD). Everything else is hidden while the mode is on.
/// </summary>
public static class HudVisibilityPolicy
{
    /// <summary>True if the addon should be hidden while the mode is on.</summary>
    public static bool ShouldHide(string addonName)
        => !string.IsNullOrEmpty(addonName) && !ChatAddons.IsChat(addonName);
}
