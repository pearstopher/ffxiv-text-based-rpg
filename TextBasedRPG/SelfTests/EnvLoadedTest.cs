using Dalamud.Plugin.SelfTest;

namespace TextBasedRPG.SelfTests;

/// <summary>Proves the self-test pipeline works: reports the running build and whether a character is logged in.</summary>
internal sealed class EnvLoadedTest : GameSelfTest
{
    public override string Id => "env-loaded";

    public override string Description => "Plugin loaded; reports build and login state";

    public override SelfTestStepResult RunStep()
        => this.Pass($"{Plugin.DescribeBuild()}; logged in: {(Service.ClientState.IsLoggedIn ? "yes" : "no")}");
}
