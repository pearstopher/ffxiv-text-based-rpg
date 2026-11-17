namespace TextBasedRPG.Core;

/// <summary>What <c>/trpg &lt;args&gt;</c> asks for.</summary>
public enum CommandAction
{
    /// <summary>No arguments or <c>toggle</c>: switch the mode.</summary>
    Toggle,

    /// <summary><c>on</c>: turn the mode on.</summary>
    On,

    /// <summary><c>off</c>: turn the mode off.</summary>
    Off,

    /// <summary><c>status</c>: print the mode, 3D, FPS cap and CPU.</summary>
    Status,

    /// <summary><c>settings</c> or <c>config</c>: open the settings window.</summary>
    Settings,

    /// <summary><c>help</c>: list the commands.</summary>
    Help,

    /// <summary>Anything else: print usage.</summary>
    Unknown,
}

/// <summary>Parses the arguments of <see cref="Commands.Main"/> / <see cref="Commands.Alias"/>.</summary>
public static class CommandParser
{
    /// <summary>Usage text, shown by <c>help</c> and for unknown input.</summary>
    public static readonly IReadOnlyList<string> Usage =
    [
        $"{Commands.Alias} — turn text-based RPG mode on or off",
        $"{Commands.Alias} on | off — turn it on / off",
        $"{Commands.Alias} status — show the mode, frame rate cap and CPU use",
        $"{Commands.Alias} settings — open the settings",
        $"{Commands.Alias} help — show this list ({Commands.Main} works too)",
    ];

    /// <summary>Parses the argument string Dalamud passes to the command handler.</summary>
    public static CommandAction Parse(string? args)
        => (args ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "" or "toggle" => CommandAction.Toggle,
            "on" => CommandAction.On,
            "off" => CommandAction.Off,
            "status" => CommandAction.Status,
            "settings" or "config" => CommandAction.Settings,
            "help" or "?" => CommandAction.Help,
            _ => CommandAction.Unknown,
        };
}
