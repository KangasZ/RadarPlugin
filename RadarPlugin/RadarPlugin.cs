using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using RadarPlugin.UI;

namespace RadarPlugin;

public class RadarPlugin : IDalamudPlugin
{
    public string Name => "Radar Plugin";
    private readonly Configuration Configuration;
    private readonly RadarPlugin Plugin;
    private readonly RadarLogic radarLogic;
    private readonly PluginCommands pluginCommands;
    private readonly MainUi mainUi;
    private readonly MobEditUi mobEditUi;
    private readonly LocalMobsUi localMobsUi;
    private readonly RadarHelpers radarHelpers;
    private readonly TypeConfigurator typeConfiguratorUi;

    public RadarPlugin(
        DalamudPluginInterface pluginInterface,
        CommandManager commandManager,
        ObjectTable objectTable,
        Condition condition,
        ClientState clientState,
        GameGui gameGui)
    {
        Plugin = this;
        // Services and DI
        Configuration = new Configuration(pluginInterface);
        radarHelpers = new RadarHelpers(Configuration, clientState, condition);

        // UI
        mobEditUi = new MobEditUi(pluginInterface, Configuration, radarHelpers);
        localMobsUi = new LocalMobsUi(pluginInterface, Configuration, objectTable, mobEditUi, radarHelpers);
        typeConfiguratorUi = new TypeConfigurator(pluginInterface, Configuration, radarHelpers);
        mainUi = new MainUi(pluginInterface, Configuration, localMobsUi, clientState, radarHelpers, typeConfiguratorUi);
        
        // Command manager
        pluginCommands = new PluginCommands(commandManager, mainUi, Configuration);
        radarLogic = new RadarLogic(pluginInterface, Configuration, objectTable, condition, clientState, gameGui, radarHelpers);
    }

    public void Dispose()
    {
        // UI
        mainUi.Dispose();
        localMobsUi.Dispose();
        mobEditUi.Dispose();
        
        // Customer services
        pluginCommands.Dispose();
        radarLogic.Dispose();
    }
}