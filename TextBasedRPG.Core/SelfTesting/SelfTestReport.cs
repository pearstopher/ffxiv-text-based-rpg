using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TextBasedRPG.Core.SelfTesting;

/// <summary>Outcome of one in-game self-test step.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SelfTestStatus>))]
public enum SelfTestStatus
{
    /// <summary>The check succeeded.</summary>
    Pass,

    /// <summary>The check failed: a bug to fix.</summary>
    Fail,

    /// <summary>The step didn't finish within its time limit.</summary>
    Timeout,
}

/// <summary>One step's result.</summary>
/// <param name="Id">Stable id, listed in docs/IN_GAME_TEST_PLAN.md.</param>
/// <param name="Status">Pass, Fail or Timeout.</param>
/// <param name="Detail">What was checked or measured, or why it failed.</param>
/// <param name="DurationMs">How long the step ran.</param>
public sealed record SelfTestResult(string Id, SelfTestStatus Status, string Detail, long DurationMs);

/// <summary>
/// A whole self-test run: written to <c>selftest-results.json</c> for Claude to read, and summarised in chat and /xllog.
/// </summary>
public sealed record SelfTestReport(DateTimeOffset StartedAt, DateTimeOffset FinishedAt, string Build, IReadOnlyList<SelfTestResult> Results)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // keeps paths and quotes readable in the file
    };

    /// <summary>Number of results with the given status.</summary>
    public int Count(SelfTestStatus status) => this.Results.Count(r => r.Status == status);

    /// <summary>True when every step passed.</summary>
    public bool AllPassed => this.Results.Count > 0 && this.Results.All(r => r.Status == SelfTestStatus.Pass);

    /// <summary>E.g. "5/6 passed, 1 failed".</summary>
    public string Summary
    {
        get
        {
            var text = new StringBuilder(string.Create(CultureInfo.InvariantCulture, $"{this.Count(SelfTestStatus.Pass)}/{this.Results.Count} passed"));
            var failed = this.Count(SelfTestStatus.Fail);
            var timedOut = this.Count(SelfTestStatus.Timeout);
            if (failed > 0)
                text.Append(CultureInfo.InvariantCulture, $", {failed} failed");
            if (timedOut > 0)
                text.Append(CultureInfo.InvariantCulture, $", {timedOut} timed out");
            return text.ToString();
        }
    }

    /// <summary>The results file content.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>Reads a results file (used by tests and tools).</summary>
    public static SelfTestReport? FromJson(string json) => JsonSerializer.Deserialize<SelfTestReport>(json, JsonOptions);

    /// <summary>Multi-line text for /xllog and the clipboard.</summary>
    public string ToText()
    {
        var text = new StringBuilder();
        text.AppendLine(CultureInfo.InvariantCulture, $"Text-Only Mode self-tests: {this.Summary}");
        text.AppendLine(CultureInfo.InvariantCulture, $"{this.Build}; started {this.StartedAt:yyyy-MM-dd HH:mm:ss zzz}");
        foreach (var r in this.Results)
            text.AppendLine(CultureInfo.InvariantCulture, $"  [{r.Status}] {r.Id} ({r.DurationMs} ms): {r.Detail}");
        return text.ToString();
    }
}
