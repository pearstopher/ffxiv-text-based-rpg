using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class DebugCommandParserTests
{
    [Theory]
    [InlineData(null, DebugAction.List)]
    [InlineData("  ", DebugAction.List)]
    [InlineData("cpu", DebugAction.Cpu)]
    [InlineData("CPU", DebugAction.Cpu)]
    [InlineData("3d off", DebugAction.Hide3D)]
    [InlineData("3D   OFF", DebugAction.Hide3D)]
    [InlineData("3d on", DebugAction.Show3D)]
    [InlineData("addons", DebugAction.Addons)]
    [InlineData("3d", DebugAction.Unknown)]
    [InlineData("3d maybe", DebugAction.Unknown)]
    [InlineData("cpu now", DebugAction.Unknown)]
    public void Parses_debug_subcommands(string? args, DebugAction expected)
    {
        Assert.Equal(expected, DebugCommandParser.Parse(args));
    }

    [Fact]
    public void Usage_lists_every_subcommand()
    {
        var usage = string.Join("\n", DebugCommandParser.Usage);
        Assert.Contains("cpu", usage);
        Assert.Contains("3d off", usage);
        Assert.Contains("addons", usage);
    }
}
