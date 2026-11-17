using System.Globalization;

namespace TextBasedRPG.Core;

/// <summary>
/// Averages over one measured period: CPU % (share of the whole machine), frames per second, and GPU % (the busiest
/// GPU 3D engine's utilisation by this process; null when it couldn't be measured).
/// </summary>
public readonly record struct PhaseResult(double CpuPercent, double Fps, double? GpuPercent = null)
{
    /// <summary>This result with the given GPU %.</summary>
    public PhaseResult WithGpu(double? gpuPercent) => this with { GpuPercent = gpuPercent };
}

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
    /// E.g. <c>baseline CPU 12.0%, GPU 40%, 60 FPS → 3D off CPU 3.0%, GPU 2%, 240 FPS: CPU −9.0 points (−75%), GPU −38 points (−95%)</c>.
    /// A GPU value that couldn't be measured shows as "GPU n/a", and then there's no GPU saving.
    /// </summary>
    public static string Compare(PhaseResult baseline, string testName, PhaseResult test)
    {
        var text = string.Create(
            CultureInfo.InvariantCulture,
            $"baseline {Period(baseline)} → {testName} {Period(test)}: CPU {Change(baseline.CpuPercent, test.CpuPercent, "0.0")}");
        if (baseline.GpuPercent is { } before && test.GpuPercent is { } after)
            text += ", GPU " + Change(before, after, "0");
        return text;
    }

    /// <summary>
    /// One line for the chat summary, e.g. <c>text-only mode: CPU 4.4% → 1.6% (−63%), GPU 11% → 1% (−94%), 30 → 10 FPS</c>.
    /// GPU is left out when either period lacks it.
    /// </summary>
    public static string Summary(PhaseResult baseline, string testName, PhaseResult test)
    {
        var text = string.Create(
            CultureInfo.InvariantCulture,
            $"{testName}: CPU {baseline.CpuPercent:0.0}% → {test.CpuPercent:0.0}% ({Saving(baseline.CpuPercent, test.CpuPercent)})");
        if (baseline.GpuPercent is { } before && test.GpuPercent is { } after)
            text += string.Create(CultureInfo.InvariantCulture, $", GPU {before:0}% → {after:0}% ({Saving(before, after)})");
        return text + string.Create(CultureInfo.InvariantCulture, $", {baseline.Fps:0} → {test.Fps:0} FPS");
    }

    private static string Saving(double before, double after)
        => Signed(before > 0 ? (after - before) / before * 100 : 0, "0") + "%";

    private static string Period(PhaseResult r)
    {
        var gpu = r.GpuPercent is { } g ? g.ToString("0", CultureInfo.InvariantCulture) + "%" : "n/a";
        return string.Create(CultureInfo.InvariantCulture, $"CPU {r.CpuPercent:0.0}%, GPU {gpu}, {r.Fps:0} FPS");
    }

    /// <summary>E.g. "−9.0 points (−75%)".</summary>
    private static string Change(double before, double after, string pointsFormat)
    {
        var points = after - before;
        var percent = before > 0 ? points / before * 100 : 0;
        return $"{Signed(points, pointsFormat)} points ({Signed(percent, "0")}%)";
    }

    private static string Signed(double value, string format)
        => (value < 0 ? "−" : "+") + Math.Abs(value).ToString(format, CultureInfo.InvariantCulture);
}
