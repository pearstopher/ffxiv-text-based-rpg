using System.Globalization;

namespace TextBasedRPG.Core;

/// <summary>Average CPU % (share of the whole machine) and frames per second over one measured period.</summary>
public readonly record struct PhaseResult(double CpuPercent, double Fps);

/// <summary>
/// Measures one period: call <see cref="Start"/>, count frames with <see cref="Frame"/>, then <see cref="Stop"/>.
/// Used by the measurement self-tests (baseline vs 3D off, baseline vs full mode).
/// </summary>
public sealed class PhaseMeter
{
    private readonly int processorCount;
    private TimeSpan startCpu;
    private TimeSpan startWall;
    private int frames;

    /// <param name="processorCount">Logical processors (<c>Environment.ProcessorCount</c>).</param>
    public PhaseMeter(int processorCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(processorCount);
        this.processorCount = processorCount;
    }

    /// <summary>Starts the period at the given process CPU time and wall-clock time.</summary>
    public void Start(TimeSpan processorTime, TimeSpan wallTime)
    {
        this.startCpu = processorTime;
        this.startWall = wallTime;
        this.frames = 0;
    }

    /// <summary>Counts one frame.</summary>
    public void Frame() => this.frames++;

    /// <summary>Time since <see cref="Start"/>.</summary>
    public TimeSpan Elapsed(TimeSpan wallTime) => wallTime - this.startWall;

    /// <summary>Ends the period and returns its averages.</summary>
    public PhaseResult Stop(TimeSpan processorTime, TimeSpan wallTime)
    {
        var wallMs = (wallTime - this.startWall).TotalMilliseconds;
        if (wallMs <= 0)
            return new PhaseResult(0, 0);

        var cpuMs = Math.Max(0, (processorTime - this.startCpu).TotalMilliseconds);
        var cpu = Math.Clamp(cpuMs / (wallMs * this.processorCount) * 100, 0, 100);
        return new PhaseResult(cpu, this.frames * 1000.0 / wallMs);
    }
}

/// <summary>Text for comparing a baseline period with a test period.</summary>
public static class MeasurementText
{
    /// <summary>
    /// E.g. <c>baseline CPU 12.0%, 60 FPS → 3D off CPU 3.0%, 240 FPS: CPU −9.0 points (−75%)</c>.
    /// </summary>
    public static string Compare(PhaseResult baseline, string testName, PhaseResult test)
    {
        var points = test.CpuPercent - baseline.CpuPercent;
        var percent = baseline.CpuPercent > 0 ? points / baseline.CpuPercent * 100 : 0;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"baseline CPU {baseline.CpuPercent:0.0}%, {baseline.Fps:0} FPS → {testName} CPU {test.CpuPercent:0.0}%, {test.Fps:0} FPS: CPU {Signed(points)} points ({Signed(percent, "0")}%)");
    }

    private static string Signed(double value, string format = "0.0")
        => (value < 0 ? "−" : "+") + Math.Abs(value).ToString(format, CultureInfo.InvariantCulture);
}
