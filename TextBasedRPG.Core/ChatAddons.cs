namespace TextBasedRPG.Core;

/// <summary>
/// Names of the game's chat window addons (the same list Simple Tweaks' "Hide Chat" and Fader use).
/// </summary>
public static class ChatAddons
{
    /// <summary>The chat window frame: tabs and the text input.</summary>
    public const string ChatLog = "ChatLog";

    /// <summary>The main chat log panel (the messages of the first window).</summary>
    public const string MainPanel = "ChatLogPanel_0";

    /// <summary>Every chat addon: the frame and the four log panels (panels 1–3 are the extra chat windows players can pop out).</summary>
    public static readonly IReadOnlyList<string> All =
        [ChatLog, MainPanel, "ChatLogPanel_1", "ChatLogPanel_2", "ChatLogPanel_3"];

    /// <summary>Chat addons that are always shown in a normal game (the others depend on the player's chat setup).</summary>
    public static readonly IReadOnlyList<string> AlwaysShown = [ChatLog, MainPanel];

    /// <summary>True for any chat addon name (ordinal, case-sensitive like the game's names).</summary>
    public static bool IsChat(string addonName) => AllSet.Contains(addonName);

    // IsChat runs for every drawn addon on each HUD scan (4 times a second): a set lookup, not a list search.
    private static readonly HashSet<string> AllSet = new(All, StringComparer.Ordinal);
}
