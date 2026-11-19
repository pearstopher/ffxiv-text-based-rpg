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

/// <summary>Parses the arguments of <see cref="Commands.Main"/>.</summary>
public static class CommandParser
{
    /// <summary>Usage text, shown by <c>help</c> and for unknown input.</summary>
    public static IReadOnlyList<string> Usage =>
    [
        Strings.Get("command.usage.toggle", Commands.Main),
        Strings.Get("command.usage.onOff", Commands.Main),
        Strings.Get("command.usage.status", Commands.Main),
        Strings.Get("command.usage.settings", Commands.Main),
        Strings.Get("command.usage.help", Commands.Main),
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
