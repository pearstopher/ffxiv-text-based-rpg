using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class CommandParserTests
{
    [Theory]
    [InlineData(null, CommandAction.Toggle)]
    [InlineData("", CommandAction.Toggle)]
    [InlineData("   ", CommandAction.Toggle)]
    [InlineData("toggle", CommandAction.Toggle)]
    [InlineData("on", CommandAction.On)]
    [InlineData("ON", CommandAction.On)]
    [InlineData("  off  ", CommandAction.Off)]
    [InlineData("status", CommandAction.Status)]
    [InlineData("settings", CommandAction.Settings)]
    [InlineData("Config", CommandAction.Settings)]
    [InlineData("help", CommandAction.Help)]
    [InlineData("?", CommandAction.Help)]
    [InlineData("onn", CommandAction.Unknown)]
    [InlineData("on now", CommandAction.Unknown)]
    public void Parses_subcommands(string? args, CommandAction expected)
    {
        Assert.Equal(expected, CommandParser.Parse(args));
    }

    [Fact]
    public void Usage_mentions_every_subcommand_and_the_command()
    {
        var usage = string.Join("\n", CommandParser.Usage);
        foreach (var word in new[] { "on", "off", "status", "settings", "help", Commands.Main })
            Assert.Contains(word, usage);
    }

    [Fact]
    public void Player_commands_are_the_main_command_and_the_tests()
    {
        Assert.Equal(["/trpg", "/trpgtest"], Commands.Player);
        Assert.Equal(["/trpgdebug"], Commands.Dev);
    }
}
