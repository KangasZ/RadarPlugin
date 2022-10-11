using System;
using Dalamud.Game.Command;

namespace RadarPlugin;

public class PluginCommands : IDisposable
{
    private CommandManager commandManager;
    private PluginUi pluginUi;
    
    public PluginCommands(CommandManager commandManager, PluginUi pluginUi)
    {
        this.pluginUi = pluginUi;
        this.commandManager = commandManager;
        this.commandManager.AddHandler("/radar", new CommandInfo(SettingsCommand)
        {
            HelpMessage = "Opens configuration",
            ShowInHelp = true
        });
    }
    
    private void SettingsCommand(string command, string args)
    {
        pluginUi.OpenUi();
    }
    
    public void Dispose()
    {
        commandManager.RemoveHandler("/radar");
    }
}