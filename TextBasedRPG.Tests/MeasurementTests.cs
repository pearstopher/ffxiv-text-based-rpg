using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class MeasurementTests
{
    private static TimeSpan S(double seconds) => TimeSpan.FromSeconds(seconds);

    [Fact]
    public void Phase_meter_averages_cpu_and_counts_fps()
    {
        var meter = new PhaseMeter(processorCount: 8);
        meter.Start(S(100), S(50));
        for (var i = 0; i < 600; i++)
            meter.Frame();
        var result = meter.Stop(S(108), S(60)); // 8 CPU-seconds over 10 s on 8 cores = 10%

        Assert.Equal(10, result.CpuPercent, 6);
        Assert.Equal(60, result.Fps, 6);
        Assert.Equal(S(10), meter.Elapsed(S(60)));
    }

    [Fact]
    public void Phase_meter_handles_zero_time_and_restarts()
    {
        var meter = new PhaseMeter(1);
        meter.Start(S(0), S(5));
        meter.Frame();
        Assert.Equal(new PhaseResult(0, 0), meter.Stop(S(1), S(5)));

        meter.Start(S(1), S(5));
        meter.Frame();
        var result = meter.Stop(S(1.5), S(6));
        Assert.Equal(50, result.CpuPercent, 6);
        Assert.Equal(1, result.Fps, 6);
    }

    [Fact]
    public void Compare_shows_both_periods_and_the_saving()
    {
        var text = MeasurementText.Compare(new PhaseResult(12, 60), "3D off", new PhaseResult(3, 240));
        Assert.Equal("baseline CPU 12.0%, GPU n/a, 60 FPS → 3D off CPU 3.0%, GPU n/a, 240 FPS: CPU −9.0 points (−75%)", text);
    }

    [Fact]
    public void Compare_shows_an_increase_with_a_plus_sign()
    {
        var text = MeasurementText.Compare(new PhaseResult(5, 60), "test", new PhaseResult(6, 60));
        Assert.EndsWith("CPU +1.0 points (+20%)", text);
    }

    [Fact]
    public void Compare_with_a_zero_baseline_does_not_divide_by_zero()
    {
        Assert.EndsWith("(+0%)", MeasurementText.Compare(new PhaseResult(0, 0), "x", new PhaseResult(1, 1)));
    }

    [Fact]
    public void Compare_includes_the_gpu_saving_when_both_periods_have_it()
    {
        var text = MeasurementText.Compare(new PhaseResult(12, 60, 40), "3D off", new PhaseResult(3, 240, 2));
        Assert.Equal(
            "baseline CPU 12.0%, GPU 40%, 60 FPS → 3D off CPU 3.0%, GPU 2%, 240 FPS: CPU −9.0 points (−75%), GPU −38 points (−95%)",
            text);
    }

    [Fact]
    public void Compare_shows_na_and_no_gpu_saving_when_one_period_lacks_gpu()
    {
        var text = MeasurementText.Compare(new PhaseResult(10, 30, 40), "x", new PhaseResult(5, 10));
        Assert.Contains("GPU 40%", text);
        Assert.Contains("GPU n/a", text);
        Assert.EndsWith("CPU −5.0 points (−50%)", text);
    }

    [Fact]
    public void With_gpu_keeps_cpu_and_fps()
    {
        Assert.Equal(new PhaseResult(1, 2, 3), new PhaseResult(1, 2).WithGpu(3));
    }
}
