using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using RadarPlugin.UI;

namespace RadarPlugin;

public sealed class RadarPlugin : IDalamudPlugin
{
    public string Name => "Radar Plugin";
    private RadarLogic radarLogic;
    private PluginCommands pluginCommands;
    private Configuration configInterface;
    private MainUi mainUi;
    private MobEditUi mobEditUi;
    private LocalMobsUi localMobsUi;
    private RadarHelpers radarHelpers;

    public RadarPlugin(
        DalamudPluginInterface pluginInterface,
        CommandManager commandManager,
        ObjectTable objectTable,
        Condition condition,
        ClientState clientState,
        GameGui gameGui)
    {
        // Services and DI
        configInterface = new Configuration(pluginInterface);
        radarHelpers = new RadarHelpers(configInterface, clientState);

        // UI
        mobEditUi = new MobEditUi(pluginInterface, configInterface, radarHelpers);
        localMobsUi = new LocalMobsUi(pluginInterface, configInterface, objectTable, mobEditUi, radarHelpers);
        mainUi = new MainUi(pluginInterface, configInterface, localMobsUi);
        
        // Command manager
        pluginCommands = new PluginCommands(commandManager, mainUi);
        radarLogic = new RadarLogic(pluginInterface, configInterface, objectTable, condition, clientState, gameGui, radarHelpers);
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