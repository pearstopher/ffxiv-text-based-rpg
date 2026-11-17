namespace TextBasedRPG.Core;

/// <summary>Allowed ranges for the settings, used by the settings window and to fix bad values on load.</summary>
public static class SettingLimits
{
    /// <summary>Idle frame-rate cap range.</summary>
    public const int IdleFpsMin = 1, IdleFpsMax = 30;

    /// <summary>Typing frame-rate cap range.</summary>
    public const int TypingFpsMin = 5, TypingFpsMax = 60;

    /// <summary>Clamps the idle cap into its range.</summary>
    public static int ClampIdleFps(int fps) => Math.Clamp(fps, IdleFpsMin, IdleFpsMax);

    /// <summary>Clamps the typing cap into its range.</summary>
    public static int ClampTypingFps(int fps) => Math.Clamp(fps, TypingFpsMin, TypingFpsMax);
}
