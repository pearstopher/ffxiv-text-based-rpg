using Dalamud.Plugin.SelfTest;
using TextBasedRPG.Core;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// The "Turn on when AFK" setting, end to end in the game: a real <see cref="AfkWatcher"/> and the real mode, fed a
/// simulated AFK status (going AFK for real would mean sending /afk to the server, which tests never do). Also checks
/// the player's real online status can be read. Runs whether or not the setting is ticked; skipped if the mode is on.
/// </summary>
internal sealed class AfkAutoTest : GameSelfTest
{
    private const int FramesPerStage = 5;

    // What to do at the start of each stage, the simulated AFK state, and the mode state expected at its end.
    private static readonly Stage[] Stages =
    [
        new("not AFK: stays off", Afk: false, Expect: false),
        new("goes AFK: turns on", Afk: true, Expect: true),
        new("back: turns off", Afk: false, Expect: false),
        new("on before AFK, goes AFK: stays on", Afk: true, Expect: true, Before: ModeRequest.On),
        new("on before AFK, back: stays on", Afk: false, Expect: true),
        new("goes AFK while on: stays on", Afk: true, Expect: true),
        new("turned off while AFK: stays off", Afk: true, Expect: false, Before: ModeRequest.Off),
        new("back after turning off: stays off", Afk: false, Expect: false),
        new("goes AFK again: turns on", Afk: true, Expect: true),
        new("back again: turns off", Afk: false, Expect: false),
    ];

    private readonly Plugin plugin;
    private AfkWatcher? watcher;
    private bool? simulatedAfk;
    private int stage = -1;
    private int frames;
    private string realStatus = string.Empty;

    internal AfkAutoTest(Plugin plugin) => this.plugin = plugin;

    public override string Id => "afk-auto";

    public override string Description => "Turn on when AFK: on when AFK, off when back, a mode already on stays on";

    public override int TimeoutMs => 30_000;

    public override SelfTestStepResult RunStep()
    {
        if (this.stage < 0)
            return this.Start();

        if (++this.frames < FramesPerStage)
            return SelfTestStepResult.Waiting;

        var current = Stages[this.stage];
        if (this.plugin.Mode.IsOn != current.Expect)
            return this.Fail($"Stage {this.stage + 1} \"{current.Name}\": the mode is {(this.plugin.Mode.IsOn ? "on" : "off")}. {this.realStatus}");

        if (++this.stage < Stages.Length)
        {
            this.Enter(Stages[this.stage]);
            return SelfTestStepResult.Waiting;
        }

        return this.Pass($"{Stages.Length} AFK cases behaved as expected (simulated AFK status). {this.realStatus}");
    }

    public override void CleanUp()
    {
        this.watcher?.Dispose();
        this.watcher = null;
        if (this.stage >= 0 && this.plugin.Mode.IsOn)
            this.plugin.Mode.Request(ModeRequest.Off);
        this.stage = -1;
        this.frames = 0;
        this.simulatedAfk = null;
        this.realStatus = string.Empty;
    }

    private SelfTestStepResult Start()
    {
        if (this.plugin.Mode.IsOn)
            return this.Pass("Skipped: the mode is already on (the test won't turn off your mode).");
        if (!Service.ClientState.IsLoggedIn || GameState.InTransition)
        {
            this.Detail = "Waiting for a logged-in character outside zone loads and cutscenes.";
            return SelfTestStepResult.Waiting;
        }

        if (AfkWatcher.PlayerIsAfk is not { } afk)
            return this.Fail("Couldn't read the player's online status (no local player).");
        this.realStatus = $"Real online status read: {(afk ? "AFK" : "not AFK")}.";

        this.watcher = new AfkWatcher(this.plugin.Mode, paused: () => false, isAfk: () => this.simulatedAfk, checkIntervalMs: 0)
        {
            Enabled = true,
        };
        this.stage = 0;
        this.Enter(Stages[0]);
        return SelfTestStepResult.Waiting;
    }

    private void Enter(Stage next)
    {
        if (next.Before is { } request)
            this.plugin.Mode.Request(request);
        this.simulatedAfk = next.Afk;
        this.frames = 0;
    }

    private sealed record Stage(string Name, bool Afk, bool Expect, ModeRequest? Before = null);
}
