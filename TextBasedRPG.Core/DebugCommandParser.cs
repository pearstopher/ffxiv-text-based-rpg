namespace TextBasedRPG.Core;

/// <summary>What <c>/trpgdebug &lt;args&gt;</c> asks for.</summary>
public enum DebugAction
{
    /// <summary>No arguments: list the debug subcommands.</summary>
    List,

    /// <summary><c>cpu</c>: print CPU % and FPS.</summary>
    Cpu,

    /// <summary><c>3d off</c>: hide the 3D world (set the render-disable flag directly; spike tool).</summary>
    Hide3D,

    /// <summary><c>3d on</c>: show the 3D world again.</summary>
    Show3D,

    /// <summary><c>addons</c>: dump the loaded UI addons to a file.</summary>
    Addons,

    /// <summary>Anything else.</summary>
    Unknown,
}

/// <summary>Parses the arguments of <see cref="Commands.Debug"/> (dev builds only).</summary>
public static class DebugCommandParser
{
    /// <summary>Help lines for <c>/trpgdebug</c>.</summary>
    public static readonly IReadOnlyList<string> Usage =
    [
        $"{Commands.Debug} cpu — CPU use and frame rate",
        $"{Commands.Debug} 3d off | on — hide / show the 3D world directly (render flag only)",
        $"{Commands.Debug} addons — write the loaded UI addons to addons.json",
    ];

    /// <summary>Parses the argument string.</summary>
    public static DebugAction Parse(string? args)
    {
        var words = (args ?? string.Empty).Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words switch
        {
            [] => DebugAction.List,
            ["cpu"] => DebugAction.Cpu,
            ["3d", "off"] => DebugAction.Hide3D,
            ["3d", "on"] => DebugAction.Show3D,
            ["addons"] => DebugAction.Addons,
            _ => DebugAction.Unknown,
        };
    }
}
