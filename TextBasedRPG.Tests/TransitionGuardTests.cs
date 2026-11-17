using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class TransitionGuardTests
{
    [Fact]
    public void Allows_changes_when_no_transition_has_happened()
    {
        var guard = new TransitionGuard();
        Assert.True(guard.CanFlip);
        Assert.True(guard.Update(inTransition: false, nowMs: 0));
    }

    [Fact]
    public void Blocks_during_a_transition()
    {
        var guard = new TransitionGuard();
        Assert.False(guard.Update(inTransition: true, nowMs: 1_000));
        Assert.False(guard.Update(inTransition: true, nowMs: 60_000));
        Assert.False(guard.CanFlip);
    }

    [Fact]
    public void Blocks_until_the_settle_time_after_a_transition_ends()
    {
        var guard = new TransitionGuard();
        guard.Update(inTransition: true, nowMs: 10_000);
        Assert.False(guard.Update(inTransition: false, nowMs: 10_016));
        Assert.False(guard.Update(inTransition: false, nowMs: 10_000 + TransitionGuard.SettleMs - 1));
        Assert.True(guard.Update(inTransition: false, nowMs: 10_000 + TransitionGuard.SettleMs));
    }

    [Fact]
    public void Settle_time_counts_from_the_last_transition_frame()
    {
        var guard = new TransitionGuard();
        guard.Update(inTransition: true, nowMs: 0);
        guard.Update(inTransition: false, nowMs: 2_000);
        guard.Update(inTransition: true, nowMs: 2_500); // a second load right after the first
        Assert.False(guard.Update(inTransition: false, nowMs: 5_000));
        Assert.True(guard.Update(inTransition: false, nowMs: 5_500));
    }
}
