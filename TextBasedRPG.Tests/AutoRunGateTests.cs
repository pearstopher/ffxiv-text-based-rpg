using TextBasedRPG.Core.SelfTesting;

namespace TextBasedRPG.Tests;

public class AutoRunGateTests
{
    [Fact]
    public void Starts_after_being_logged_in_for_the_settle_time()
    {
        var gate = new AutoRunGate(0);
        Assert.Equal(AutoRunDecision.Wait, gate.Update(1_000, loggedIn: true));
        Assert.Equal(AutoRunDecision.Wait, gate.Update(1_000 + AutoRunGate.SettleMs - 1, loggedIn: true));
        Assert.Equal(AutoRunDecision.Start, gate.Update(1_000 + AutoRunGate.SettleMs, loggedIn: true));
    }

    [Fact]
    public void Logging_out_restarts_the_settle_time()
    {
        var gate = new AutoRunGate(0);
        gate.Update(1_000, loggedIn: true);
        gate.Update(2_000, loggedIn: false);
        Assert.Equal(AutoRunDecision.Wait, gate.Update(4_500, loggedIn: true));
        Assert.Equal(AutoRunDecision.Start, gate.Update(4_500 + AutoRunGate.SettleMs, loggedIn: true));
    }

    [Fact]
    public void Gives_up_when_nobody_logs_in()
    {
        var gate = new AutoRunGate(10_000);
        Assert.Equal(AutoRunDecision.Wait, gate.Update(10_000 + AutoRunGate.GiveUpMs - 1, loggedIn: false));
        Assert.Equal(AutoRunDecision.GiveUp, gate.Update(10_000 + AutoRunGate.GiveUpMs, loggedIn: false));
    }

    [Fact]
    public void A_late_login_still_starts()
    {
        var gate = new AutoRunGate(0);
        gate.Update(AutoRunGate.GiveUpMs - 1, loggedIn: true);
        Assert.Equal(AutoRunDecision.Start, gate.Update(AutoRunGate.GiveUpMs - 1 + AutoRunGate.SettleMs, loggedIn: true));
    }
}
