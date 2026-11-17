using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ModeMessagesTests
{
    [Fact]
    public void Turning_on_tells_how_to_go_back()
    {
        var (text, isError) = ModeMessages.For(ModeChange.TurnedOn, isOn: true);
        Assert.Contains("/trpg", text);
        Assert.False(isError);
    }

    [Fact]
    public void Refusal_is_an_error()
    {
        Assert.True(ModeMessages.For(ModeChange.RefusedNotLoggedIn, isOn: false).IsError);
    }

    [Theory]
    [InlineData(true, "already on")]
    [InlineData(false, "already off")]
    public void Unchanged_says_the_current_state(bool isOn, string expected)
    {
        Assert.Contains(expected, ModeMessages.For(ModeChange.Unchanged, isOn).Text);
    }

    [Fact]
    public void Status_lists_every_part()
    {
        var text = ModeMessages.Status(true, true, false, 10, 42, "CPU 2.0% (avg 2.1%), FPS 10");
        Assert.Equal("Text-based RPG mode: on · 3D off · FPS cap 10 · 42 UI elements hidden · CPU 2.0% (avg 2.1%), FPS 10", text);
    }

    [Fact]
    public void Status_mentions_a_pending_3d_change_and_no_cap()
    {
        var text = ModeMessages.Status(true, false, true, 0, 0, "x");
        Assert.Contains("3D on (changes after the loading screen)", text);
        Assert.Contains("no FPS cap", text);
    }
}
