using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class FpsTargetTests
{
    [Fact]
    public void Idle_cap_when_not_typing()
    {
        var target = new FpsTarget();
        Assert.Equal(10, target.Choose(10, 30, typing: false, nowMs: 0));
    }

    [Fact]
    public void Typing_cap_while_typing_and_shortly_after()
    {
        var target = new FpsTarget();
        Assert.Equal(30, target.Choose(10, 30, typing: true, nowMs: 1_000));
        Assert.Equal(30, target.Choose(10, 30, typing: false, nowMs: 1_000 + FpsTarget.LingerMs - 1));
        Assert.Equal(10, target.Choose(10, 30, typing: false, nowMs: 1_000 + FpsTarget.LingerMs));
    }

    [Fact]
    public void Typing_cap_is_never_below_the_idle_cap()
    {
        var target = new FpsTarget();
        Assert.Equal(20, target.Choose(20, 5, typing: true, nowMs: 0));
    }

    [Fact]
    public void Reset_forgets_recent_typing()
    {
        var target = new FpsTarget();
        target.Choose(10, 30, typing: true, nowMs: 0);
        target.Reset();
        Assert.Equal(10, target.Choose(10, 30, typing: false, nowMs: 1));
    }
}
