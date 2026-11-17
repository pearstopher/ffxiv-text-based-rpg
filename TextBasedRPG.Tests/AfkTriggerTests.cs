using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class AfkTriggerTests
{
    [Fact]
    public void Fires_once_when_the_player_goes_afk()
    {
        var trigger = new AfkTrigger();
        Assert.False(trigger.Update(false));
        Assert.True(trigger.Update(true));
        Assert.False(trigger.Update(true)); // still AFK: no repeat, so turning the mode off sticks
        Assert.False(trigger.Update(false));
        Assert.True(trigger.Update(true));  // next time they go AFK
    }

    [Fact]
    public void Already_afk_when_watching_starts_does_not_fire()
    {
        var trigger = new AfkTrigger();
        Assert.False(trigger.Update(true));

        trigger.Update(false);
        trigger.Reset();
        Assert.False(trigger.Update(true));
    }

    [Fact]
    public void No_character_is_not_a_starting_point_for_going_afk()
    {
        var trigger = new AfkTrigger();
        trigger.Update(false);
        Assert.False(trigger.Update(null)); // logged out / loading
        Assert.False(trigger.Update(true)); // logging back in already AFK doesn't count
    }

    [Fact]
    public void Afk_status_is_row_17()
    {
        Assert.Equal(17u, AfkTrigger.AfkOnlineStatus);
    }
}
