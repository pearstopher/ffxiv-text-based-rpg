namespace TextBasedRPG.Core;

/// <summary>
/// The plugin's slash commands. The single source of truth for command names: the plugin registers these,
/// and DocumentationTests check that each one is documented.
/// </summary>
public static class Commands
{
    /// <summary>Main command.</summary>
    public const string Main = "/trpg";

    /// <summary>Runs the in-game self-tests and prints the measured savings (every build).</summary>
    public const string SelfTest = "/trpgtest";

    /// <summary>Dev builds only: debug tools (<c>/trpgdebug &lt;subcommand&gt;</c>).</summary>
    public const string Debug = "/trpgdebug";

    /// <summary>Commands every player has; documented in docs/COMMANDS.md.</summary>
    public static readonly IReadOnlyList<string> Player = [Main, SelfTest];

    /// <summary>Dev-build commands, hidden from /xlhelp; documented in DEVELOPMENT.md.</summary>
    public static readonly IReadOnlyList<string> Dev = [Debug];
}
