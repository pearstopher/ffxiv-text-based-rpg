using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dalamud.Plugin.SelfTest;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core.SelfTesting;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// Dev runner: runs the same <see cref="GameSelfTest"/> steps Dalamud's Self-Test window runs, one at a time on the
/// framework thread, then writes <c>selftest-results.json</c> to the plugin config folder (Claude reads it), logs the
/// report and prints a chat summary.
/// </summary>
internal sealed class SelfTestRunner : IDisposable
{
    internal const string ResultsFileName = "selftest-results.json";

    private readonly Plugin plugin;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly List<SelfTestResult> results = [];
    private List<GameSelfTest> steps = [];
    private int index;
    private StepClock? stepClock;
    private DateTimeOffset startedAt;

    /// <summary>Dev plugin (loaded from a Dev Plugin Location) or a Debug build.</summary>
    internal static bool IsDevBuild => Service.PluginInterface.IsDev || Plugin.BuildConfiguration == "Debug";

    internal static string ResultsPath => Path.Combine(Service.PluginInterface.GetPluginConfigDirectory(), ResultsFileName);

    internal SelfTestRunner(Plugin plugin) => this.plugin = plugin;

    internal bool IsRunning { get; private set; }

    /// <summary>The last finished run, if any.</summary>
    internal SelfTestReport? LastReport { get; private set; }

    /// <summary>Starts a run. Returns false if one is already running or this isn't a dev build.</summary>
    internal bool Start()
    {
        if (!IsDevBuild || this.IsRunning)
            return false;

        this.steps = SelfTestCatalog.Create(this.plugin);
        this.results.Clear();
        this.index = 0;
        this.stepClock = null;
        this.startedAt = DateTimeOffset.Now;
        this.IsRunning = true;
        Service.Framework.Update += this.OnUpdate;
        Service.Log.Information($"Self-tests started ({this.steps.Count} steps)");
        return true;
    }

    private void OnUpdate(IFramework framework)
    {
        if (this.index >= this.steps.Count)
        {
            this.Finish();
            return;
        }

        var step = this.steps[this.index];
        var now = this.clock.ElapsedMilliseconds;
        this.stepClock ??= new StepClock(now, step.TimeoutMs);

        SelfTestStatus? status;
        try
        {
            status = step.RunStep() switch
            {
                SelfTestStepResult.Pass => SelfTestStatus.Pass,
                SelfTestStepResult.Fail => SelfTestStatus.Fail,
                _ => this.stepClock.TimedOut(now) ? SelfTestStatus.Timeout : null,
            };
        }
        catch (Exception ex)
        {
            status = SelfTestStatus.Fail;
            step.Detail = $"Threw {ex.GetType().Name}: {ex.Message}";
            Service.Log.Error(ex, $"Self-test {step.Id} threw");
        }

        if (status is null)
            return;

        if (status == SelfTestStatus.Timeout && string.IsNullOrEmpty(step.Detail))
            step.Detail = $"No result within {step.TimeoutMs} ms";

        this.results.Add(new SelfTestResult(step.Id, status.Value, step.Detail, this.stepClock.Elapsed(now)));
        CleanUpSafely(step);
        this.index++;
        this.stepClock = null;
    }

    private void Finish()
    {
        Service.Framework.Update -= this.OnUpdate;
        this.IsRunning = false;

        var report = new SelfTestReport(this.startedAt, DateTimeOffset.Now, Plugin.DescribeBuild(), [.. this.results]);
        this.LastReport = report;

        try
        {
            File.WriteAllText(ResultsPath, report.ToJson());
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, $"Could not write {ResultsPath}");
        }

        Service.Log.Information(report.ToText());
        var summary = $"Text-Only Mode self-tests: {report.Summary}.";
        if (report.AllPassed)
        {
            Service.ChatGui.Print(summary);
            return;
        }

        Service.ChatGui.PrintError(summary);
        foreach (var r in report.Results)
        {
            if (r.Status != SelfTestStatus.Pass)
                Service.ChatGui.PrintError($"  ✗ {r.Id}: {r.Detail}");
        }
    }

    private static void CleanUpSafely(GameSelfTest step)
    {
        try
        {
            step.CleanUp();
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, $"Self-test {step.Id} cleanup threw");
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        if (this.IsRunning && this.index < this.steps.Count)
            CleanUpSafely(this.steps[this.index]);
        this.IsRunning = false;
    }
}
