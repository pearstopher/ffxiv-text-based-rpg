using System;
using System.Diagnostics;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using TextBasedRPG.Core;
using TextBasedRPG.Core.SelfTesting;

namespace TextBasedRPG.SelfTests;

/// <summary>
/// Dev builds only: the <c>/trpgtest</c> command, and the automatic run after each load (so `dotnet build` →
/// Dalamud auto-reload → results file, with nobody typing anything).
/// </summary>
internal sealed class SelfTestTriggers : IDisposable
{
    private const long CheckIntervalMs = 1_000;

    private readonly SelfTestRunner runner;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly AutoRunGate? gate;
    private readonly bool commandRegistered;
    private long nextCheckMs;

    internal SelfTestTriggers(SelfTestRunner runner, Configuration config)
    {
        this.runner = runner;
        if (!SelfTestRunner.IsDevBuild)
            return;

        Service.CommandManager.AddHandler(Commands.SelfTest, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Dev builds: run Text-Based RPG's in-game self-tests.",
            ShowInHelp = false,
        });
        this.commandRegistered = true;

        if (config.DevRunSelfTestsOnLoad)
        {
            this.gate = new AutoRunGate(this.clock.ElapsedMilliseconds);
            Service.Framework.Update += this.OnUpdate;
        }
    }

    private void OnCommand(string command, string args)
    {
        if (!this.runner.Start())
            Service.ChatGui.PrintError("Text-Based RPG self-tests are already running.");
    }

    private void OnUpdate(IFramework framework)
    {
        var now = this.clock.ElapsedMilliseconds;
        if (now < this.nextCheckMs)
            return;
        this.nextCheckMs = now + CheckIntervalMs;

        switch (this.gate!.Update(now, Service.ClientState.IsLoggedIn))
        {
            case AutoRunDecision.Start:
                Service.Framework.Update -= this.OnUpdate;
                this.runner.Start();
                break;
            case AutoRunDecision.GiveUp:
                Service.Framework.Update -= this.OnUpdate;
                Service.Log.Information("Self-test auto-run skipped: no character logged in.");
                break;
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= this.OnUpdate;
        if (this.commandRegistered)
            Service.CommandManager.RemoveHandler(Commands.SelfTest);
    }
}
