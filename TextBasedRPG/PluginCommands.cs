using System;
using Dalamud.Game.Command;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>Registers <c>/trpg</c> with Dalamud and dispatches its subcommands.</summary>
internal sealed class PluginCommands : IDisposable
{
    private readonly Plugin plugin;

    internal PluginCommands(Plugin plugin)
    {
        this.plugin = plugin;
        Service.CommandManager.AddHandler(Commands.Main, new CommandInfo(this.OnCommand)
        {
            HelpMessage = Strings.Get("command.help.main", Commands.Main),
        });
    }

    private void OnCommand(string command, string args)
    {
        switch (CommandParser.Parse(args))
        {
            case CommandAction.Toggle:
                this.Request(ModeRequest.Toggle);
                break;
            case CommandAction.On:
                this.Request(ModeRequest.On);
                break;
            case CommandAction.Off:
                this.Request(ModeRequest.Off);
                break;
            case CommandAction.Status:
                Service.ChatGui.Print(this.plugin.StatusText());
                break;
            case CommandAction.Help:
                PrintUsage();
                break;
            case CommandAction.Unknown:
                Service.ChatGui.PrintError(Strings.Get("command.unknown", args.Trim()));
                PrintUsage();
                break;
            case CommandAction.Settings:
                this.plugin.Ui.ToggleSettings();
                break;
        }
    }

    private void Request(ModeRequest request) => this.plugin.Mode.RequestAndReport(request);

    private static void PrintUsage()
    {
        foreach (var line in CommandParser.Usage)
            Service.ChatGui.Print(line);
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(Commands.Main);
    }
}
