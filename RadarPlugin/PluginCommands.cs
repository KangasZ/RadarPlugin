using System;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using RadarPlugin.UI;

namespace RadarPlugin;

public class PluginCommands : IDisposable
{
    private readonly ICommandManager commandManager;
    private readonly MainUi mainUi;
    private readonly Configuration configInterface;

    public PluginCommands(ICommandManager commandManager, MainUi mainUi, Configuration configuration)
    {
        this.mainUi = mainUi;
        this.commandManager = commandManager;
        this.configInterface = configuration;
        this.commandManager.AddHandler("/radar", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration. Subcommands: /radar [ debug ]",
            ShowInHelp = true
        });
        this.commandManager.AddHandler("/radarcfg", new CommandInfo(RadarCfgCommand)
        {
            HelpMessage = "Opens configuration. Subcommands: /radarcfg [ load ] { fileName }",
            ShowInHelp = true
        });
    }

    private void RadarCfgCommand(string command, string arguments)
    {
        var regex = Regex.Match(arguments, "^(\\w+) ?(.*)");
        var subcommand = regex.Success && regex.Groups.Count > 1 ? regex.Groups[1].Value : string.Empty;
        switch (subcommand.ToLower())
        {
            case "load":
            {
                configInterface.LoadConfig(regex.Groups[2].Value);
                break;
            }
        }
    }


    private void SettingsCommand(string command, string args)
    {
        if (string.IsNullOrEmpty(args))
        { 
            mainUi.OpenUi();
            return;
        }

        var regex = Regex.Match(args, "^(\\w+) ?(.*)");
        var subcommand = regex.Success && regex.Groups.Count > 1 ? regex.Groups[1].Value : string.Empty;
        switch (subcommand.ToLower())
        {
            case "debug":
            {
                configInterface.cfg.DebugMode = !configInterface.cfg.DebugMode;
                break;
            }
        }
    }

    public void Dispose()
    {
        commandManager.RemoveHandler("/radar");
        commandManager.RemoveHandler("/radarcfg");

    }
}