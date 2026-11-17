using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Plugin.SelfTest;
using TextBasedRPG.Core;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// The mode does what it promises: with it on, 3D is off, only the chat is drawn, and the frame rate is near the idle
/// cap; with it off again, 3D is back on.
/// </summary>
internal sealed class ModeCycleTest : GameSelfTest
{
    private const long SettleMs = 2_000;   // let the 3D request apply and the frame cap settle
    // Count frames over this long. A non-chat addon only counts as "still drawn" if it is drawn both at the start
    // and the end of this period (the HUD hider re-scans every 250 ms, so a newly opened addon can show briefly).
    private const long MeasureMs = 3_000;

    private readonly Plugin plugin;
    private readonly Stopwatch clock = new();
    private int phase;
    private int frames;
    private HashSet<string> drawnNonChat = [];
    private string inModeDetail = string.Empty;
    private bool turnedOn;

    internal ModeCycleTest(Plugin plugin) => this.plugin = plugin;

    public override string Id => "mode-cycle";

    public override string Description => "In the mode: 3D off, only chat drawn, FPS capped; after: 3D back";

    public override int TimeoutMs => 40_000;

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

                if (this.plugin.Mode.Request(ModeRequest.On) != ModeChange.TurnedOn)
                    return this.Fail("The mode didn't turn on.");
                this.turnedOn = true;
                this.clock.Restart();
                this.phase = 1;
                return SelfTestStepResult.Waiting;

            case 1: // settle
                if (this.clock.ElapsedMilliseconds < SettleMs)
                    return SelfTestStepResult.Waiting;
                this.drawnNonChat = DrawnNonChat();
                this.frames = 0;
                this.clock.Restart();
                this.phase = 2;
                return SelfTestStepResult.Waiting;

            case 2: // measure frames, re-check drawn addons
                this.frames++;
                if (this.clock.ElapsedMilliseconds < MeasureMs)
                    return SelfTestStepResult.Waiting;
                return this.CheckInMode();

            case 3: // mode off; give 3D a moment to come back
                if (++this.frames < 5 || (this.plugin.Render.IsWaiting && this.clock.ElapsedMilliseconds < 10_000))
                    return SelfTestStepResult.Waiting;
                if (RenderDisableManager.HasOwnRequest)
                    return this.Fail($"{this.inModeDetail} After: our 3D-off request is still in place.");
                var note = RenderFlag.Is3DDisabled == true ? " (3D still off: another plugin, e.g. AutoRetainer, wants it off)" : string.Empty;
                return this.Pass($"{this.inModeDetail} After: 3D request removed{note}.");

            default:
                return this.Fail("Unexpected phase.");
        }
    }

    private SelfTestStepResult CheckInMode()
    {
        var fps = this.frames * 1000.0 / this.clock.ElapsedMilliseconds;
        var cap = this.plugin.Limiter.IdleFps;
        var stillDrawn = DrawnNonChat().Intersect(this.drawnNonChat).ToList();
        var chatMissing = ChatAddons.AlwaysShown.Where(n => UiAddons.Find(n) is not { IsVisible: true, RootNodeVisible: true }).ToList();
        var problems = new List<string>();

        if (RenderFlag.Is3DDisabled != true)
            problems.Add("3D is still drawn");
        if (stillDrawn.Count > 0)
            problems.Add($"still drawn: {string.Join(", ", stillDrawn)}");
        if (chatMissing.Count > 0)
            problems.Add($"chat not drawn: {string.Join(", ", chatMissing)}");
        if (fps > cap * 1.5 + 1)
            problems.Add($"{fps:0.0} FPS is above the {cap} FPS cap");

        var guarded = this.plugin.Hud.Guarded.Count > 0 ? $" (re-hidden before each draw: {string.Join(", ", this.plugin.Hud.Guarded)})" : string.Empty;
        this.inModeDetail = $"In mode: 3D off, {this.plugin.Hud.HiddenCount} UI elements hidden{guarded}, chat drawn, {fps:0.0} FPS (cap {cap}).";

        this.plugin.Mode.Request(ModeRequest.Off);
        this.turnedOn = false;
        if (problems.Count > 0)
            return this.Fail("In mode: " + string.Join("; ", problems));

        this.frames = 0;
        this.clock.Restart();
        this.phase = 3;
        return SelfTestStepResult.Waiting;
    }

    public override void CleanUp()
    {
        if (this.turnedOn)
            this.plugin.Mode.Request(ModeRequest.Off);
        this.turnedOn = false;
        this.phase = 0;
        this.frames = 0;
        this.drawnNonChat = [];
        this.inModeDetail = string.Empty;
    }

    private static HashSet<string> DrawnNonChat()
        => UiAddons.Snapshot()
            .Where(a => a.IsVisible && a.RootNodeVisible && HudVisibilityPolicy.ShouldHide(a.Name))
            .Select(a => a.Name)
            .ToHashSet();
}
