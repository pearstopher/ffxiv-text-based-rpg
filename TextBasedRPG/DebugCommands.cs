using System;
using System.IO;
using System.Linq;
using Dalamud.Game.Command;
using TextBasedRPG.Core;
using TextBasedRPG.SelfTests;

namespace TextBasedRPG;

/// <summary>Dev builds only: <c>/trpgdebug &lt;subcommand&gt;</c>, hidden from /xlhelp. Documented in DEVELOPMENT.md.</summary>
internal sealed class DebugCommands : IDisposable
{
    private readonly Plugin plugin;
    private readonly bool registered;

    // True while 3D is hidden by `/trpgdebug 3d off`, so unloading the plugin shows it again.
    private bool hid3D;

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
            case DebugAction.Hide3D:
                this.Set3DHidden(true);
                break;
            case DebugAction.Show3D:
                this.Set3DHidden(false);
                break;
            case DebugAction.Addons:
                DumpAddons();
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

    private void Set3DHidden(bool hidden)
    {
        if (GameState.InTransition)
        {
            Service.ChatGui.PrintError("Not now: the game is loading a zone or playing a cutscene. Try again in a few seconds.");
            return;
        }

        if (!RenderFlag.Set3DDisabled(hidden))
        {
            Service.ChatGui.PrintError("The game's render manager isn't available.");
            return;
        }

        this.hid3D = hidden;
        Service.Log.Information($"Debug: 3D rendering {(hidden ? "off" : "on")} (render flag set directly)");
        Service.ChatGui.Print(hidden
            ? $"3D world hidden. {Commands.Debug} 3d on to show it again."
            : "3D world shown.");
    }

    private static void DumpAddons()
    {
        var addons = UiAddons.Snapshot();
        var path = Path.Combine(Service.PluginInterface.GetPluginConfigDirectory(), "addons.json");
        try
        {
            File.WriteAllText(path, AddonInfo.ToJson(addons, DateTimeOffset.Now, Plugin.DescribeBuild()));
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, $"Could not write {path}");
            Service.ChatGui.PrintError($"Could not write {path}: {ex.Message}");
            return;
        }

        Service.ChatGui.Print($"{addons.Count} addons ({addons.Count(a => a.IsVisible && a.RootNodeVisible)} drawn) written to {path}");
    }

    private static void PrintUsage()
    {
        foreach (var line in DebugCommandParser.Usage)
            Service.ChatGui.Print(line);
    }

    public void Dispose()
    {
        if (this.hid3D && RenderFlag.Is3DDisabled == true)
            RenderFlag.Set3DDisabled(false);

        if (this.registered)
            Service.CommandManager.RemoveHandler(Commands.Debug);
    }
}
