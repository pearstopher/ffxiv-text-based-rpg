using System;
using Dalamud.Game.Command;
using TextBasedRPG.Core;
using TextBasedRPG.SelfTests;

namespace TextBasedRPG;

/// <summary>Dev builds only: <c>/trpgdebug &lt;subcommand&gt;</c>, hidden from /xlhelp. Documented in DEVELOPMENT.md.</summary>
internal sealed class DebugCommands : IDisposable
{
    private readonly Plugin plugin;
    private readonly bool registered;

    internal DebugCommands(Plugin plugin)
    {
        this.plugin = plugin;
        if (!SelfTestRunner.IsDevBuild)
            return;

        Service.CommandManager.AddHandler(Commands.Debug, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Dev builds: Text-Based RPG debug tools.",
            ShowInHelp = false,
        });
        this.registered = true;
    }

    private void OnCommand(string command, string args)
    {
        switch (DebugCommandParser.Parse(args))
        {
            case DebugAction.Cpu:
                Service.ChatGui.Print(this.plugin.Cpu.Describe());
                break;
            case DebugAction.Unknown:
                Service.ChatGui.PrintError($"Unknown debug option \"{args.Trim()}\".");
                PrintUsage();
                break;
            case DebugAction.List:
                PrintUsage();
                break;
            default:
                Service.ChatGui.Print("Not implemented yet.");
                break;
        }
    }

    private static void PrintUsage()
    {
        foreach (var line in DebugCommandParser.Usage)
            Service.ChatGui.Print(line);
    }

    public void Dispose()
    {
        if (this.registered)
            Service.CommandManager.RemoveHandler(Commands.Debug);
    }
}
