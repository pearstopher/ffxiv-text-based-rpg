using System;
using Dalamud.Game.Command;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>Registers <c>/textrpg</c> and <c>/trpg</c> with Dalamud and dispatches their subcommands.</summary>
internal sealed class PluginCommands : IDisposable
{
    internal PluginCommands()
    {
        Service.CommandManager.AddHandler(Commands.Main, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Turn text-based RPG mode on or off (only the chat stays, low CPU). /textrpg help for more.",
        });
        Service.CommandManager.AddHandler(Commands.Alias, new CommandInfo(this.OnCommand)
        {
            HelpMessage = $"Short for {Commands.Main}.",
        });
    }

    private void OnCommand(string command, string args)
    {
        switch (CommandParser.Parse(args))
        {
            case CommandAction.Help:
                PrintUsage();
                break;
            case CommandAction.Unknown:
                Service.ChatGui.PrintError($"Unknown option \"{args.Trim()}\".");
                PrintUsage();
                break;
            default:
                Service.ChatGui.Print("Text-Based RPG: not implemented yet.");
                break;
        }
    }

    private static void PrintUsage()
    {
        foreach (var line in CommandParser.Usage)
            Service.ChatGui.Print(line);
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(Commands.Alias);
        Service.CommandManager.RemoveHandler(Commands.Main);
    }
}
