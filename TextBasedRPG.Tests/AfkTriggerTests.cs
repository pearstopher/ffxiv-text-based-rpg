using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class AfkTriggerTests
{
    [Fact]
    public void Mode_off_going_afk_turns_it_on_and_coming_back_turns_it_off()
    {
        var trigger = new AfkTrigger();
        Assert.Equal(AfkAction.None, trigger.Update(false, modeOn: false));
        Assert.Equal(AfkAction.TurnOn, trigger.Update(true, modeOn: false));
        trigger.TurnedOn();
        Assert.Equal(AfkAction.None, trigger.Update(true, modeOn: true)); // still AFK
        Assert.Equal(AfkAction.TurnOff, trigger.Update(false, modeOn: true));
        Assert.False(trigger.TurnedOnByAfk);
    }

    [Fact]
    public void Mode_already_on_stays_on_through_afk_and_back()
    {
        var trigger = new AfkTrigger();
        trigger.Update(false, modeOn: true);
        Assert.Equal(AfkAction.None, trigger.Update(true, modeOn: true));
        Assert.Equal(AfkAction.None, trigger.Update(false, modeOn: true));
    }

    [Fact]
    public void Turning_the_mode_off_while_afk_sticks_until_the_next_afk()
    {
        var trigger = new AfkTrigger();
        trigger.Update(false, modeOn: false);
        trigger.Update(true, modeOn: false);
        trigger.TurnedOn();

        trigger.ModeChangedByPlayer(); // player typed /trpg off
        Assert.Equal(AfkAction.None, trigger.Update(true, modeOn: false));
        Assert.Equal(AfkAction.None, trigger.Update(false, modeOn: false));
        Assert.Equal(AfkAction.TurnOn, trigger.Update(true, modeOn: false));
    }

    [Fact]
    public void Mode_switched_back_on_by_the_player_is_theirs_and_stays_on()
    {
        var trigger = new AfkTrigger();
        trigger.Update(false, modeOn: false);
        trigger.Update(true, modeOn: false);
        trigger.TurnedOn();
        trigger.ModeChangedByPlayer(); // off...
        trigger.ModeChangedByPlayer(); // ...and on again by the player
        Assert.Equal(AfkAction.None, trigger.Update(false, modeOn: true));
    }

    [Fact]
    public void Already_afk_when_watching_starts_does_not_turn_it_on()
    {
        var trigger = new AfkTrigger();
        Assert.Equal(AfkAction.None, trigger.Update(true, modeOn: false));

        trigger.Update(false, modeOn: false);
        trigger.Reset();
        Assert.Equal(AfkAction.None, trigger.Update(true, modeOn: false));
    }

    [Fact]
    public void Loading_screens_without_a_character_are_ignored()
    {
        var trigger = new AfkTrigger();
        trigger.Update(false, modeOn: false);
        trigger.Update(true, modeOn: false);
        trigger.TurnedOn();
        Assert.Equal(AfkAction.None, trigger.Update(null, modeOn: true));
        Assert.Equal(AfkAction.None, trigger.Update(true, modeOn: true));
        Assert.Equal(AfkAction.TurnOff, trigger.Update(false, modeOn: true));
    }

    [Fact]
    public void Afk_status_is_row_17()
    {
        Assert.Equal(17u, AfkTrigger.AfkOnlineStatus);
    }
}
