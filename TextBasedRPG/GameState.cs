using Dalamud.Game.ClientState.Conditions;

namespace TextBasedRPG;

/// <summary>Small read-only questions about the game's state, answered with Dalamud services.</summary>
internal static class GameState
{
    /// <summary>
    /// True while the game is loading a zone or playing a cutscene. Changing the 3D render flag then can crash the
    /// game (XIV_HolyPlugin: <c>SetPixelShader</c> on the render thread), so it is only changed outside these.
    /// </summary>
    internal static bool InTransition
    {
        get
        {
            var c = Service.Condition;
            return c[ConditionFlag.BetweenAreas]
                || c[ConditionFlag.BetweenAreas51]
                || c[ConditionFlag.OccupiedInCutSceneEvent]
                || c[ConditionFlag.WatchingCutscene]
                || c[ConditionFlag.WatchingCutscene78];
        }
    }
}
