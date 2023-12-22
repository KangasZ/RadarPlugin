using System;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using RadarPlugin.UI;

namespace RadarPlugin;

public class PluginCommands : IDisposable
{
    private readonly ICommandManager commandManager;
    private readonly MainUi mainUi;
    private readonly Configuration configInterface;
    private readonly IChatGui chatGui;

    public PluginCommands(ICommandManager commandManager, MainUi mainUi, Configuration configuration, IChatGui chatGui)
    {
        this.mainUi = mainUi;
        this.chatGui = chatGui;
        this.commandManager = commandManager;
        this.configInterface = configuration;
        this.commandManager.AddHandler("/radar", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration. Subcommands: /radar [ showall | showdebug ]",
            ShowInHelp = true
        });
        this.commandManager.AddHandler("/radarcfg", new CommandInfo(RadarCfgCommand)
        {
            HelpMessage = "Loads config manually. Subcommands: /radarcfg [ load ] { fileName }",
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
            case "showall":
            {
                configInterface.cfg.DebugMode = !configInterface.cfg.DebugMode;
                
                var activatedString = configInterface.cfg.DebugMode ? "Enabled" : "Disabled";
                var seString = new SeStringBuilder();
                seString.Append($"Radar Plugin: Show All Entities Feature {activatedString}");
                var chatEntry = new XivChatEntry()
                {
                    Type = XivChatType.Echo,
                    Message = seString.Build()
                };
                chatGui.Print(chatEntry);
                
                break;
            }
            case "showdebug":
            {
                configInterface.cfg.DebugText = !configInterface.cfg.DebugText;
                
                var activatedString = configInterface.cfg.DebugText ? "Enabled" : "Disabled";
                var seString = new SeStringBuilder();
                seString.Append($"Radar Plugin: Debug Text Feature {activatedString}");
                var chatEntry = new XivChatEntry()
                {
                    Type = XivChatType.Echo,
                    Message = seString.Build()
                };
                chatGui.Print(chatEntry);
                
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