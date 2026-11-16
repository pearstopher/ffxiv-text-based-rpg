using System.Text.RegularExpressions;
using TextBasedRPG.Core;
using TextBasedRPG.Tests.Support;

namespace TextBasedRPG.Tests;

/// <summary>
/// Fails when a command or setting isn't documented, so the user-facing docs stay current.
/// </summary>
public partial class DocumentationTests
{
    public static TheoryData<string> PlayerCommands => new(Commands.Player);

    public static TheoryData<string> DevCommands => new(Commands.Dev);

    public static TheoryData<string> Settings => new(SettingNames());

    [Theory]
    [MemberData(nameof(PlayerCommands))]
    public void Player_commands_are_in_COMMANDS_md(string command)
    {
        Assert.Contains($"`{command}`", Repo.Read("docs", "COMMANDS.md"));
    }

    [Theory]
    [MemberData(nameof(DevCommands))]
    public void Dev_commands_are_in_DEVELOPMENT_md(string command)
    {
        Assert.Contains($"`{command}", Repo.Read("DEVELOPMENT.md"));
    }

    [Theory]
    [MemberData(nameof(Settings))]
    public void Settings_are_in_SETTINGS_md(string setting)
    {
        Assert.Contains($"`{setting}`", Repo.Read("docs", "SETTINGS.md"));
    }

    [Fact]
    public void Settings_are_found_in_Configuration_cs()
    {
        // Guards the parser below: if it stops finding properties, the setting checks would pass vacuously.
        Assert.Contains("Version", SettingNames());
    }

    /// <summary>
    /// Public settable properties of <c>Configuration</c>, read from the source file (the plugin project needs
    /// Dalamud, so the tests can't load the type itself).
    /// </summary>
    private static List<string> SettingNames()
        => PublicProperty().Matches(Repo.Read("TextBasedRPG", "Configuration.cs")).Select(m => m.Groups[1].Value).ToList();

    [GeneratedRegex(@"public\s+[\w<>\[\]?,\s]+?\s+(\w+)\s*\{\s*get;\s*set;\s*\}")]
    private static partial Regex PublicProperty();
}
