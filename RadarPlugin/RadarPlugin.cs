using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace RadarPlugin.UI;

public sealed class RadarPlugin : IDalamudPlugin
{
    public string Name => "Radar Plugin";
    private RadarLogic radarLogic { get; set; }
    private PluginCommands pluginCommands { get; set; }
    private Configuration configInterface { get; set; }
    private MainUi MainUi { get; set; }
    private MobEditUi mobEditUi { get; set; }
    private LocalMobsUi localMobsUi { get; set; }
    private ObjectTable objectTable { get; set; }
    private Condition condition { get; set; }
    private ClientState clientState { get; set; }

    public RadarPlugin(
        DalamudPluginInterface pluginInterface,
        CommandManager commandManager,
        ObjectTable objectTable,
        Condition condition,
        ClientState clientState)
    {
        // Services and DI
        pluginInterface.Create<Services>(); // Todo: Remove this
        this.objectTable = objectTable;
        this.condition = condition;
        configInterface = new Configuration(pluginInterface);
        
        // UI
        mobEditUi = new MobEditUi(pluginInterface, configInterface);
        localMobsUi = new LocalMobsUi(pluginInterface, configInterface, objectTable, mobEditUi);
        MainUi = new MainUi(pluginInterface, configInterface, localMobsUi);
        
        // Command manager
        pluginCommands = new PluginCommands(commandManager, MainUi);
        this.clientState = clientState;
        radarLogic = new RadarLogic(pluginInterface, configInterface, this.objectTable, this.condition, this.clientState);
    }

    public void Dispose()
    {
        // UI
        MainUi.Dispose();
        localMobsUi.Dispose();
        mobEditUi.Dispose();
        
        // Customer services
        pluginCommands.Dispose();
        radarLogic.Dispose();
    }
}