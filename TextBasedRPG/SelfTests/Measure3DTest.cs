namespace TextBasedRPG.SelfTests;

/// <summary>CPU, GPU and FPS with 3D on (baseline) vs 3D off (render flag only; no FPS cap or HUD hiding).</summary>
internal sealed class Measure3DTest : MeasureTestBase
{
    private bool original;

    public override string Id => "measure-3d";

    public override string Description => "CPU, GPU and FPS: 3D on vs 3D off (10 s each)" + FocusNote;

    protected override string TestName => "3D off";

    protected override bool Apply()
    {
        if (RenderFlag.Is3DDisabled is not { } current)
        {
            this.Detail = "Render.Manager.Instance() is null.";
            return false;
        }

        if (current)
        {
            this.Detail = "3D is already off (another plugin or /trpgdebug), so there's no baseline to compare.";
            return false;
        }

        this.original = current;
        return RenderFlag.Set3DDisabled(true);
    }

    protected override bool Undo() => RenderFlag.Set3DDisabled(this.original) && RenderFlag.Is3DDisabled == this.original;
}
