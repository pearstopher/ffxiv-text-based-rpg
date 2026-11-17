using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace TextBasedRPG;

/// <summary>
/// Direct access to the game's "skip 3D rendering" flag, <c>Render.Manager.Is3DRenderingDisabled</c>
/// (FFXIVClientStructs). With it set, the game clears the screen, runs post-effects and draws the UI, but skips the
/// whole 3D world. See docs/design/research.md §1.
/// </summary>
/// <remarks>
/// The mode itself goes through <c>RenderDisableManager</c> (cooperative with other plugins); this direct access is
/// for the debug command and self-tests. Framework thread only.
/// </remarks>
internal static unsafe class RenderFlag
{
    /// <summary>True when the render manager exists (the game has finished starting).</summary>
    internal static bool Available => Manager.Instance() != null;

    /// <summary>The flag's current value, or null if the render manager isn't available.</summary>
    internal static bool? Is3DDisabled
    {
        get
        {
            var manager = Manager.Instance();
            return manager == null ? null : manager->Is3DRenderingDisabled;
        }
    }

    /// <summary>Writes the flag. Returns false if the render manager isn't available.</summary>
    internal static bool Set3DDisabled(bool disabled)
    {
        var manager = Manager.Instance();
        if (manager == null)
            return false;

        manager->Is3DRenderingDisabled = disabled;
        return true;
    }
}
