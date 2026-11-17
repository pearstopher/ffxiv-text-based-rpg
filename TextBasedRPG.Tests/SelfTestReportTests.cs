using TextBasedRPG.Core.SelfTesting;

namespace TextBasedRPG.Tests;

public class SelfTestReportTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    private static SelfTestReport Report(params SelfTestResult[] results)
        => new(Start, Start.AddSeconds(5), "Debug build, built 2026-11-16 09:59:00, from C:\\x\\TextBasedRPG.dll", results);

    [Fact]
    public void Summary_counts_passes_failures_and_timeouts()
    {
        var report = Report(
            new SelfTestResult("a", SelfTestStatus.Pass, "ok", 1),
            new SelfTestResult("b", SelfTestStatus.Fail, "bad", 2),
            new SelfTestResult("c", SelfTestStatus.Timeout, "slow", 3));

        Assert.Equal("1/3 passed, 1 failed, 1 timed out", report.Summary);
        Assert.False(report.AllPassed);
    }

    [Fact]
    public void All_passed_needs_at_least_one_result()
    {
        Assert.False(Report().AllPassed);
        Assert.True(Report(new SelfTestResult("a", SelfTestStatus.Pass, "ok", 1)).AllPassed);
        Assert.Equal("1/1 passed", Report(new SelfTestResult("a", SelfTestStatus.Pass, "ok", 1)).Summary);
    }

    [Fact]
    public void Json_round_trips_with_readable_status_and_paths()
    {
        var report = Report(new SelfTestResult("env-loaded", SelfTestStatus.Fail, "it said \"no\"", 12));
        var json = report.ToJson();

        Assert.Contains("\"Status\": \"Fail\"", json);
        Assert.Contains("C:\\\\x\\\\TextBasedRPG.dll", json);
        var back = SelfTestReport.FromJson(json)!;
        Assert.Equal(report.Build, back.Build);
        Assert.Equal(report.StartedAt, back.StartedAt);
        Assert.Equal(report.Results, back.Results);
    }

    [Fact]
    public void Text_lists_every_result()
    {
        var text = Report(new SelfTestResult("a", SelfTestStatus.Pass, "fine", 5), new SelfTestResult("b", SelfTestStatus.Fail, "broken", 7)).ToText();

        Assert.Contains("1/2 passed, 1 failed", text);
        Assert.Contains("[Pass] a (5 ms): fine", text);
        Assert.Contains("[Fail] b (7 ms): broken", text);
    }

    [Theory]
    [InlineData(1000, 1000, false)]
    [InlineData(1000, 5999, false)]
    [InlineData(1000, 6000, true)]
    [InlineData(1000, 500, false)] // clock went backwards: not timed out, elapsed 0
    public void Step_clock_times_out_after_the_limit(long started, long now, bool timedOut)
    {
        var clock = new StepClock(started, 5000);
        Assert.Equal(timedOut, clock.TimedOut(now));
        Assert.True(clock.Elapsed(now) >= 0);
    }

    [Fact]
    public void Step_clock_rejects_a_zero_timeout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StepClock(0, 0));
    }
}
