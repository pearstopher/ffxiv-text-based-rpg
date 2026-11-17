using Dalamud.Plugin.SelfTest;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// Checks on the live game that <c>Render.Manager.Is3DRenderingDisabled</c> is reachable and keeps a value we write,
/// then restores it. Waits until no zone load or cutscene is in progress (changing the flag then can crash).
/// </summary>
internal sealed class RenderFlagTest : GameSelfTest
{
    private int phase;
    private bool original;
    private bool changed;

    public override string Id => "render-flag";

    public override string Description => "3D render flag can be set and restored";

    public override int TimeoutMs => 20_000;

    public override SelfTestStepResult RunStep()
    {
        switch (this.phase)
        {
            case 0:
                if (GameState.InTransition)
                    return SelfTestStepResult.Waiting; // try again next frame
                if (RenderFlag.Is3DDisabled is not { } current)
                    return this.Fail("Render.Manager.Instance() is null.");

                this.original = current;
                RenderFlag.Set3DDisabled(true);
                this.changed = true;
                this.phase = 1;
                return SelfTestStepResult.Waiting;

            case 1:
                // One frame later: did the value stick?
                var stuck = RenderFlag.Is3DDisabled == true;
                RenderFlag.Set3DDisabled(this.original);
                this.phase = 2;
                if (!stuck)
                    return this.Fail("Wrote true, but the flag read back false on the next frame.");
                return SelfTestStepResult.Waiting;

            default:
                if (RenderFlag.Is3DDisabled != this.original)
                    return this.Fail($"Could not restore the flag to {this.original}.");
                this.changed = false;
                return this.Pass($"Set to true, read back true next frame, restored to {this.original}.");
        }
    }

    public override void CleanUp()
    {
        if (this.changed)
            RenderFlag.Set3DDisabled(this.original);
        this.changed = false;
        this.phase = 0;
    }
}
