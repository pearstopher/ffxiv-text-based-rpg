using System.Collections.Generic;

namespace TextBasedRPG.SelfTests;

/// <summary>The plugin's in-game self-tests, in run order.</summary>
internal static class SelfTestCatalog
{
    /// <summary>
    /// Fresh step instances. Dalamud's Self-Test window and the dev runner each get their own set, so a run in one
    /// can't disturb the other's step state.
    /// </summary>
    internal static List<GameSelfTest> Create(Plugin plugin) =>
    [
        new EnvLoadedTest(),
        new ChatVisibleTest(),
        new RenderFlagTest(),
        new RestoreOnOffTest(plugin),
        new ModeCycleTest(plugin),

        // Measurements last: they take ~25 s each.
        new Measure3DTest(),
        new MeasureModeTest(plugin),
    ];
}
