using Dalamud.Plugin.SelfTest;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// Base for the plugin's in-game checks. Each one is a Dalamud <see cref="ISelfTestStep"/>, so it runs in Dalamud's
/// own Self-Test window, and also in the plugin's dev runner (<see cref="SelfTestRunner"/>), which writes the results file.
/// </summary>
/// <remarks>
/// <see cref="RunStep"/> is called once per frame on the framework thread until it returns Pass or Fail.
/// Steps must never send anything to the game server, and must undo anything they change in <see cref="CleanUp"/>.
/// </remarks>
internal abstract class GameSelfTest : ISelfTestStep
{
    /// <summary>Stable id, listed in docs/IN_GAME_TEST_PLAN.md (a unit test checks).</summary>
    public abstract string Id { get; }

    /// <summary>One line: what the step checks.</summary>
    public abstract string Description { get; }

    /// <summary>Name shown in Dalamud's Self-Test window.</summary>
    public string Name => $"{this.Id}: {this.Description}";

    /// <summary>What was checked or measured, or why it failed. Written to the results file.</summary>
    public string Detail { get; internal set; } = string.Empty;

    /// <summary>How long the dev runner lets the step run before recording a timeout.</summary>
    public virtual int TimeoutMs => 10_000;

    /// <inheritdoc/>
    public abstract SelfTestStepResult RunStep();

    /// <inheritdoc/>
    public virtual void CleanUp()
    {
    }

    /// <summary>Records <paramref name="detail"/> and passes.</summary>
    protected SelfTestStepResult Pass(string detail)
    {
        this.Detail = detail;
        return SelfTestStepResult.Pass;
    }

    /// <summary>Records <paramref name="detail"/> and fails.</summary>
    protected SelfTestStepResult Fail(string detail)
    {
        this.Detail = detail;
        return SelfTestStepResult.Fail;
    }
}
