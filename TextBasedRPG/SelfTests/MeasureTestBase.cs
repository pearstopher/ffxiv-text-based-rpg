using System;
using System.Diagnostics;
using Dalamud.Plugin.SelfTest;
using TextBasedRPG.Core;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// Measures CPU, GPU and FPS for a baseline period, applies a change, waits for it to settle, measures again, and undoes
/// the change. Passes whenever it completes: the numbers are information, written to the step's detail.
/// </summary>
internal abstract class MeasureTestBase : GameSelfTest
{
    private enum Phase { WaitForSafe, Baseline, Settle, Test, Restore }

    private readonly Stopwatch clock = new();
    private Process? process;
    private GpuMeter? gpu;
    private string? gpuError;
    private PhaseMeter meter = new(Environment.ProcessorCount);
    private Phase phase;
    private PhaseResult baseline;
    private bool applied;

    /// <summary>Length of each measured period.</summary>
    protected virtual TimeSpan MeasureFor => TimeSpan.FromSeconds(10);

    /// <summary>Length of the measured period with the change applied (defaults to <see cref="MeasureFor"/>).</summary>
    protected virtual TimeSpan TestFor => this.MeasureFor;

    /// <summary>Wait after applying the change before measuring.</summary>
    protected virtual TimeSpan SettleFor => TimeSpan.FromSeconds(3);

    /// <summary>Short name of the measured state, e.g. "3D off".</summary>
    protected abstract string TestName { get; }

    public override int TimeoutMs => (int)(this.MeasureFor.TotalMilliseconds + this.TestFor.TotalMilliseconds + this.SettleFor.TotalMilliseconds) + 20_000;

    /// <summary>A reason to skip the measurement (passes with that detail), or null to run it.</summary>
    protected virtual string? SkipReason() => null;

    /// <summary>Applies the change being measured. Return false (and set Detail) if it can't be applied.</summary>
    protected abstract bool Apply();

    /// <summary>Undoes the change. Return false if it couldn't be undone (a failure).</summary>
    protected abstract bool Undo();

    public override SelfTestStepResult RunStep()
    {
        this.meter.Frame();
        switch (this.phase)
        {
            case Phase.WaitForSafe:
                if (!Service.ClientState.IsLoggedIn || GameState.InTransition)
                {
                    this.Detail = "Waiting for a logged-in character outside zone loads and cutscenes.";
                    return SelfTestStepResult.Waiting;
                }

                if (this.SkipReason() is { } reason)
                    return this.Pass(reason);

                this.process = Process.GetCurrentProcess();
                this.gpu = GpuMeter.TryCreate(out this.gpuError);
                this.clock.Restart();
                this.StartPhase(Phase.Baseline);
                return SelfTestStepResult.Waiting;

            case Phase.Baseline:
                if (this.meter.Elapsed(this.clock.Elapsed) < this.MeasureFor)
                    return SelfTestStepResult.Waiting;

                this.baseline = this.StopPhase();
                if (GameState.InTransition)
                    return this.Fail("A zone load or cutscene started during the baseline; run again.");
                if (!this.Apply())
                    return SelfTestStepResult.Fail;

                this.applied = true;
                this.StartPhase(Phase.Settle);
                return SelfTestStepResult.Waiting;

            case Phase.Settle:
                if (this.meter.Elapsed(this.clock.Elapsed) >= this.SettleFor)
                    this.StartPhase(Phase.Test);
                return SelfTestStepResult.Waiting;

            case Phase.Test:
                if (this.meter.Elapsed(this.clock.Elapsed) < this.TestFor)
                    return SelfTestStepResult.Waiting;

                var test = this.StopPhase();
                this.phase = Phase.Restore;
                this.Detail = MeasurementText.Compare(this.baseline, this.TestName, test)
                    + (this.gpuError is { } why ? $" ({why})" : string.Empty);
                return SelfTestStepResult.Waiting;

            default:
                if (GameState.InTransition)
                    return SelfTestStepResult.Waiting; // undo once the load is over
                var undone = this.Undo();
                this.applied = !undone;
                return undone ? SelfTestStepResult.Pass : this.Fail($"Could not undo \"{this.TestName}\". {this.Detail}");
        }
    }

    public override void CleanUp()
    {
        if (this.applied)
            this.Undo();
        this.applied = false;
        this.phase = Phase.WaitForSafe;
        this.meter = new PhaseMeter(Environment.ProcessorCount);
        this.process?.Dispose();
        this.process = null;
        this.gpu?.Dispose();
        this.gpu = null;
        this.gpuError = null;
    }

    private void StartPhase(Phase next)
    {
        this.process!.Refresh();
        this.meter.Start(this.process.TotalProcessorTime, this.clock.Elapsed);
        this.gpu?.Start();
        this.phase = next;
    }

    private PhaseResult StopPhase()
    {
        this.process!.Refresh();
        return this.meter.Stop(this.process.TotalProcessorTime, this.clock.Elapsed).WithGpu(this.gpu?.Stop());
    }
}
