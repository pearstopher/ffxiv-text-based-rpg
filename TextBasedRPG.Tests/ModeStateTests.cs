using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ModeStateTests
{
    [Fact]
    public void Starts_off()
    {
        Assert.False(new ModeState().IsOn);
    }

    [Fact]
    public void Toggle_switches_on_and_off()
    {
        var mode = new ModeState();
        Assert.Equal(ModeChange.TurnedOn, mode.Request(ModeRequest.Toggle, loggedIn: true));
        Assert.True(mode.IsOn);
        Assert.Equal(ModeChange.TurnedOff, mode.Request(ModeRequest.Toggle, loggedIn: true));
        Assert.False(mode.IsOn);
    }

    [Fact]
    public void On_and_off_are_idempotent()
    {
        var mode = new ModeState();
        Assert.Equal(ModeChange.Unchanged, mode.Request(ModeRequest.Off, loggedIn: true));
        mode.Request(ModeRequest.On, loggedIn: true);
        Assert.Equal(ModeChange.Unchanged, mode.Request(ModeRequest.On, loggedIn: true));
        Assert.True(mode.IsOn);
    }

    [Theory]
    [InlineData(ModeRequest.On)]
    [InlineData(ModeRequest.Toggle)]
    public void Cannot_turn_on_without_a_character(ModeRequest request)
    {
        var mode = new ModeState();
        Assert.Equal(ModeChange.RefusedNotLoggedIn, mode.Request(request, loggedIn: false));
        Assert.False(mode.IsOn);
    }

    [Fact]
    public void Can_always_turn_off()
    {
        var mode = new ModeState();
        mode.Request(ModeRequest.On, loggedIn: true);
        Assert.Equal(ModeChange.TurnedOff, mode.Request(ModeRequest.Off, loggedIn: false));
    }

    [Fact]
    public void Logout_forces_it_off()
    {
        var mode = new ModeState();
        Assert.Equal(ModeChange.Unchanged, mode.LoggedOut());
        mode.Request(ModeRequest.On, loggedIn: true);
        Assert.Equal(ModeChange.TurnedOff, mode.LoggedOut());
        Assert.False(mode.IsOn);
    }
}
