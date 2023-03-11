using System;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using RadarPlugin.UI;

namespace RadarPlugin;

public class PluginCommands : IDisposable
{
    private readonly CommandManager commandManager;
    private readonly MainUi mainUi;
    private readonly Configuration configInterface;

    public PluginCommands(CommandManager commandManager, MainUi mainUi, Configuration configuration)
    {
        this.mainUi = mainUi;
        this.commandManager = commandManager;
        this.configInterface = configuration;
        this.commandManager.AddHandler("/radar", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration. Subcommands: /radar [ debug ]",
            ShowInHelp = true
        });
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
    }
}