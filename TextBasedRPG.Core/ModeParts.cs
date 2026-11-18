namespace TextBasedRPG.Core;

/// <summary>Which parts of text-only mode are applied. The 3D world is always switched off.</summary>
/// <param name="FrameCap">Cap the frame rate.</param>
/// <param name="HideHud">Hide every UI element except the chat.</param>
public readonly record struct ModeParts(bool FrameCap, bool HideHud)
{
    /// <summary>Full text-only mode: 3D off, frame rate capped, HUD hidden.</summary>
    public static ModeParts Full { get; } = new(true, true);

    /// <summary>Only the 3D world off; the HUD and the frame rate are left alone.</summary>
    public static ModeParts WorldOnly { get; } = new(false, false);

    /// <summary>The parts for the "3D world off only" setting.</summary>
    public static ModeParts For(bool worldOffOnly) => worldOffOnly ? WorldOnly : Full;
}
