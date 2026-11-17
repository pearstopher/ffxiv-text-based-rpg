namespace TextBasedRPG.Core;

/// <summary>Chat messages for the mode, kept in one place so they're consistent and tested.</summary>
public static class ModeMessages
{
    /// <summary>The message for a mode change, and whether it's an error.</summary>
    public static (string Text, bool IsError) For(ModeChange change, bool isOn) => change switch
    {
        ModeChange.TurnedOn => ($"Text-based RPG mode on. Only the chat is left. Type {Commands.Alias} to go back.", false),
        ModeChange.TurnedOff => ("Text-based RPG mode off. Welcome back!", false),
        ModeChange.RefusedNotLoggedIn => ("Text-based RPG mode needs a logged-in character.", true),
        _ => (isOn ? $"Text-based RPG mode is already on. Type {Commands.Alias} off to go back." : "Text-based RPG mode is already off.", false),
    };

    /// <summary>Status line, e.g. "Text-based RPG mode: on · 3D off · FPS cap 10 · 42 UI elements hidden · CPU 2.1%".</summary>
    /// <param name="isOn">Mode state.</param>
    /// <param name="threeDOff">Whether our 3D-off request is applied.</param>
    /// <param name="waitingForLoad">Whether a 3D change waits for a zone load/cutscene to finish.</param>
    /// <param name="fpsCap">Frame-rate cap in use (0 = none).</param>
    /// <param name="hiddenAddons">UI elements hidden by the mode.</param>
    /// <param name="cpu">Text for CPU use and FPS.</param>
    public static string Status(bool isOn, bool threeDOff, bool waitingForLoad, int fpsCap, int hiddenAddons, string cpu)
    {
        var threeD = threeDOff ? "3D off" : "3D on";
        if (waitingForLoad)
            threeD += " (changes after the loading screen)";
        var cap = fpsCap > 0 ? $"FPS cap {fpsCap}" : "no FPS cap";
        return $"Text-based RPG mode: {(isOn ? "on" : "off")} · {threeD} · {cap} · {hiddenAddons} UI elements hidden · {cpu}";
    }
}
