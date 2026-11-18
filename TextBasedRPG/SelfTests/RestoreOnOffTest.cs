using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.SelfTest;
using TextBasedRPG.Core;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// Turns the mode on and off and checks that everything comes back: every UI element that was drawn before is drawn
/// again, our 3D-off request is gone, and the frame-rate cap is off.
/// </summary>
internal sealed class RestoreOnOffTest : GameSelfTest
{
    private const int FramesInMode = 30;

    private readonly Plugin plugin;
    private int phase;
    private int frames;
    private HashSet<string> drawnBefore = [];
    private bool turnedOn;

    internal RestoreOnOffTest(Plugin plugin) => this.plugin = plugin;

    public override string Id => "restore-on-off";

    public override string Description => "Mode on then off restores the HUD, 3D and frame rate";

    public override int TimeoutMs => 30_000;

    public override SelfTestStepResult RunStep()
    {
        switch (this.phase)
        {
            case 0:
                if (this.plugin.Mode.IsOn)
                    return this.Pass("Skipped: the mode is already on (the test won't turn off your mode).");
                if (!Service.ClientState.IsLoggedIn || GameState.InTransition)
                {
                    this.Detail = "Waiting for a logged-in character outside zone loads and cutscenes.";
                    return SelfTestStepResult.Waiting;
                }

                this.drawnBefore = DrawnAddonNames();
                if (this.plugin.Mode.Request(ModeRequest.On, ModeParts.Full) != ModeChange.TurnedOn)
                    return this.Fail("The mode didn't turn on.");
                this.turnedOn = true;
                this.phase = 1;
                return SelfTestStepResult.Waiting;

            case 1:
                // Stay in the mode long enough for the 3D request to apply and the HUD to be hidden.
                if (++this.frames < FramesInMode && !(this.plugin.Render.IsApplied && this.frames > 2))
                    return SelfTestStepResult.Waiting;

                if (this.plugin.Hud.HiddenCount == 0)
                    return this.Fail("The mode was on but no UI element was hidden.");
                this.plugin.Mode.Request(ModeRequest.Off);
                this.turnedOn = false;
                this.phase = 2;
                this.frames = 0;
                return SelfTestStepResult.Waiting;

            default:
                // Give the render request a few frames to clear.
                if (++this.frames < 5 || (this.plugin.Render.IsWaiting && this.frames < FramesInMode))
                    return SelfTestStepResult.Waiting;

                var drawnAfter = DrawnAddonNames();
                var missing = this.drawnBefore.Where(n => !drawnAfter.Contains(n) && UiAddons.Find(n) != null).ToList();
                var problems = new List<string>();
                if (missing.Count > 0)
                    problems.Add($"not drawn again: {string.Join(", ", missing)}");
                if (RenderDisableManager.HasOwnRequest)
                    problems.Add("our 3D-off request is still in place");
                if (this.plugin.Limiter.Active)
                    problems.Add("the frame-rate cap is still on");
                if (this.plugin.Hud.HiddenCount != 0)
                    problems.Add($"{this.plugin.Hud.HiddenCount} UI elements still marked hidden");

                return problems.Count == 0
                    ? this.Pass($"{this.drawnBefore.Count} drawn UI elements restored; 3D request removed; no FPS cap.")
                    : this.Fail(string.Join("; ", problems));
        }
    }

    public override void CleanUp()
    {
        if (this.turnedOn)
            this.plugin.Mode.Request(ModeRequest.Off);
        this.turnedOn = false;
        this.phase = 0;
        this.frames = 0;
        this.drawnBefore = [];
    }

    private static HashSet<string> DrawnAddonNames()
        => UiAddons.Snapshot().Where(a => a.IsVisible && a.RootNodeVisible).Select(a => a.Name).ToHashSet();
}
