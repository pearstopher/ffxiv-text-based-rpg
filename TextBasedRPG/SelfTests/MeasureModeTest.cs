using System;
using TextBasedRPG.Core;

namespace TextBasedRPG.SelfTests;

/// <summary>CPU, GPU and FPS in normal play (baseline) vs text-only mode (3D off, HUD hidden, FPS capped).</summary>
internal sealed class MeasureModeTest : MeasureTestBase
{
    private readonly Plugin plugin;

    internal MeasureModeTest(Plugin plugin) => this.plugin = plugin;

    public override string Id => "measure-mode";

    public override string Description => "CPU, GPU and FPS: normal vs text-only mode (10 s / 15 s)" + FocusNote;

    protected override string TestName => "text-only mode";

    protected override string TableTitle => "Text-only mode";


    protected override TimeSpan TestFor => TimeSpan.FromSeconds(15);

    protected override string? SkipReason()
        => this.plugin.Mode.IsOn ? "Skipped: the mode is already on, so there's no baseline (the test won't turn off your mode)." : null;

    protected override bool Apply()
    {
        if (this.plugin.Mode.Request(ModeRequest.On, ModeParts.Full) == ModeChange.TurnedOn)
            return true;

        this.Detail = "The mode didn't turn on.";
        return false;
    }

    protected override bool Undo()
    {
        this.plugin.Mode.Request(ModeRequest.Off);
        return !this.plugin.Mode.IsOn;
    }
}
