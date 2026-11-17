using System.Collections.Generic;
using Dalamud.Plugin.SelfTest;
using TextBasedRPG.Core;

namespace TextBasedRPG.SelfTests;

/// <summary>Checks the chat window's addons exist and are drawn (visible and root node shown).</summary>
internal sealed class ChatVisibleTest : GameSelfTest
{
    public override string Id => "chat-visible";

    public override string Description => "Chat window is loaded and drawn";

    public override SelfTestStepResult RunStep()
    {
        if (!Service.ClientState.IsLoggedIn)
        {
            this.Detail = "Waiting for a logged-in character (the chat only exists in game).";
            return SelfTestStepResult.Waiting;
        }

        var problems = new List<string>();
        foreach (var name in ChatAddons.AlwaysShown)
        {
            var info = UiAddons.Find(name);
            if (info == null)
                problems.Add($"{name} not loaded");
            else if (!info.IsVisible || !info.RootNodeVisible)
                problems.Add($"{name} not drawn (visible {info.IsVisible}, root node {info.RootNodeVisible})");
        }

        return problems.Count == 0
            ? this.Pass($"{string.Join(", ", ChatAddons.AlwaysShown)} visible.")
            : this.Fail(string.Join("; ", problems));
    }
}
