namespace TextBasedRPG.Core;

/// <summary>Chat messages for the mode, kept in one place so they're consistent and tested.</summary>
public static class ModeMessages
{
    /// <summary>The message for a mode change, and whether it's an error.</summary>
    /// <param name="change">What happened.</param>
    /// <param name="isOn">Mode state.</param>
    /// <param name="parts">The parts applied when the mode is on (full mode by default).</param>
    public static (string Text, bool IsError) For(ModeChange change, bool isOn, ModeParts? parts = null) => change switch
    {
        ModeChange.TurnedOn => (Strings.Get((parts ?? ModeParts.Full).HideHud ? "mode.on.full" : "mode.on.worldOnly", Commands.Alias), false),
        ModeChange.TurnedOff => (Strings.Get("mode.off"), false),
        ModeChange.RefusedNotLoggedIn => (Strings.Get("mode.notLoggedIn"), true),
        _ => (isOn ? Strings.Get("mode.alreadyOn", Commands.Alias) : Strings.Get("mode.alreadyOff"), false),
    };

    /// <summary>Status line, e.g. "Text-only mode: on · 3D off · FPS cap 10 · 42 UI elements hidden · CPU 2.1%".</summary>
    /// <param name="isOn">Mode state.</param>
    /// <param name="threeDOff">Whether our 3D-off request is applied.</param>
    /// <param name="waitingForLoad">Whether a 3D change waits for a zone load/cutscene to finish.</param>
    /// <param name="fpsCap">Frame-rate cap in use (0 = none).</param>
    /// <param name="hiddenAddons">UI elements hidden by the mode.</param>
    /// <param name="cpu">Text for CPU use and FPS.</param>
    public static string Status(bool isOn, bool threeDOff, bool waitingForLoad, int fpsCap, int hiddenAddons, string cpu)
    {
        var threeD = Strings.Get(threeDOff ? "status.3dOff" : "status.3dOn");
        if (waitingForLoad)
            threeD += Strings.Get("status.3dWaiting");
        var cap = fpsCap > 0 ? Strings.Get("status.fpsCap", fpsCap) : Strings.Get("status.noFpsCap");
        return Strings.Get("status.line", Strings.Get(isOn ? "status.on" : "status.off"), threeD, cap, hiddenAddons, cpu);
    }
}
