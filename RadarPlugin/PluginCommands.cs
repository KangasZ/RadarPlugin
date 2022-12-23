using System;
using Dalamud.Game.Command;
using RadarPlugin.UI;

namespace RadarPlugin;

public class PluginCommands : IDisposable
{
    private CommandManager commandManager;
    private MainUi mainUi;
    
    public PluginCommands(CommandManager commandManager, MainUi mainUi)
    {
        this.mainUi = mainUi;
        this.commandManager = commandManager;
        this.commandManager.AddHandler("/radar", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration",
            ShowInHelp = true
        });
    }
    
    private void SettingsCommand(string command, string args)
    {
        mainUi.OpenUi();
    }
    
    public void Dispose()
    {
        commandManager.RemoveHandler("/radar");
    }
}