using System;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Measures the game's CPU use and frame rate once per second, so the plugin can show how much the mode saves.
/// </summary>
/// <remarks>
/// Per frame it only counts the frame and checks a Stopwatch. Once a second it reads the process's total processor
/// time (one reused <see cref="Process"/>, refreshed) and feeds <see cref="CpuSampler"/>. FPS is counted here
/// rather than read from the game, so it reflects exactly the frames the game ran.
/// </remarks>
internal sealed class CpuMonitor : IDisposable
{
    private const long SampleIntervalMs = 1_000;

    private readonly Process process = Process.GetCurrentProcess();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly CpuSampler sampler = new(Environment.ProcessorCount);
    private readonly CpuComparison comparison = new();
    private long lastSampleMs;
    private int framesSinceSample;

    internal CpuMonitor()
    {
        this.sampler.Add(this.process.TotalProcessorTime, this.clock.Elapsed);
        Service.Framework.Update += this.OnUpdate;
    }

    /// <summary>CPU % of the whole machine over the last second, or null until measured.</summary>
    internal double? CurrentCpu => this.sampler.Current;

    /// <summary>Rolling average CPU % (last ~10 s), or null until measured.</summary>
    internal double? AverageCpu => this.sampler.Average;

    /// <summary>Frames per second over the last second.</summary>
    internal double Fps { get; private set; }

    /// <summary>"Normal: x% · Text mode: y% (z% less)" for this session, or null until both have data.</summary>
    internal string? Saving => this.comparison.Describe();

    /// <summary>Tells the monitor the mode switched, so samples go to the right average.</summary>
    internal void SetMode(bool on) => this.comparison.SetMode(on);

    /// <summary>Starts a fresh rolling average (e.g. after the mode switches).</summary>
    internal void ResetAverage() => this.sampler.ResetAverage();

    /// <summary>E.g. "CPU 3.1% (avg 3.4%), FPS 10".</summary>
    internal string Describe()
        => Strings.Get("cpu.describe", Format(this.CurrentCpu), Format(this.AverageCpu), $"{this.Fps:0}");

    private static string Format(double? percent) => percent is { } p ? $"{p:0.0}%" : "–";

    private void OnUpdate(IFramework framework)
    {
        this.framesSinceSample++;
        var now = this.clock.ElapsedMilliseconds;
        var elapsed = now - this.lastSampleMs;
        if (elapsed < SampleIntervalMs)
            return;

        this.process.Refresh();
        this.sampler.Add(this.process.TotalProcessorTime, this.clock.Elapsed);
        if (this.sampler.Current is { } cpu)
            this.comparison.Add(cpu);
        this.Fps = this.framesSinceSample * 1000.0 / elapsed;
        this.framesSinceSample = 0;
        this.lastSampleMs = now;
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        this.process.Dispose();
    }
}
