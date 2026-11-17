namespace TextBasedRPG.Core;

/// <summary>
/// Turns process CPU-time samples into a CPU usage percentage, so the plugin can show (and self-tests can record)
/// how much CPU the game uses with the mode off and on.
/// </summary>
/// <remarks>
/// CPU % = Δ processor time / (Δ wall time × logical cores) × 100, the same "share of the whole machine" figure
/// Task Manager shows. Rolling average over the last <see cref="WindowSize"/> intervals.
/// </remarks>
public sealed class CpuSampler
{
    private readonly int processorCount;
    private readonly double[] window;
    private int filled;
    private int next;
    private TimeSpan? lastCpu;
    private TimeSpan? lastWall;

    /// <param name="processorCount">Logical processors (<c>Environment.ProcessorCount</c>).</param>
    /// <param name="windowSize">How many intervals the rolling average covers.</param>
    public CpuSampler(int processorCount, int windowSize = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(processorCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowSize);
        this.processorCount = processorCount;
        this.window = new double[windowSize];
    }

    /// <summary>Number of intervals in the rolling average.</summary>
    public int WindowSize => this.window.Length;

    /// <summary>CPU % over the most recent interval, or null before the second sample.</summary>
    public double? Current { get; private set; }

    /// <summary>Average CPU % over the last <see cref="WindowSize"/> intervals, or null before the second sample.</summary>
    public double? Average => this.filled == 0 ? null : this.window.Take(this.filled).Average();

    /// <summary>Adds a sample: total processor time used by the process, and a monotonic wall-clock time.</summary>
    public void Add(TimeSpan processorTime, TimeSpan wallTime)
    {
        if (this.lastCpu is { } prevCpu && this.lastWall is { } prevWall)
        {
            var wall = (wallTime - prevWall).TotalMilliseconds;
            if (wall <= 0)
                return; // same or earlier time: ignore, keep the previous sample as the base

            var cpu = Math.Max(0, (processorTime - prevCpu).TotalMilliseconds);
            var percent = Math.Clamp(cpu / (wall * this.processorCount) * 100, 0, 100);
            this.Current = percent;
            this.window[this.next] = percent;
            this.next = (this.next + 1) % this.window.Length;
            this.filled = Math.Min(this.filled + 1, this.window.Length);
        }

        this.lastCpu = processorTime;
        this.lastWall = wallTime;
    }

    /// <summary>Forgets the history (e.g. when the mode switches, so averages don't mix both states).</summary>
    public void ResetAverage()
    {
        this.filled = 0;
        this.next = 0;
        this.Current = null;
    }
}
