using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RadarPlugin.RadarLogic;
using RadarPlugin.UI;

namespace RadarPlugin;

public class RadarPlugin : IDalamudPlugin
{
    public string Name => "Radar Plugin";
    private readonly Configuration.Configuration Configuration;
    private readonly RadarPlugin Plugin;
    private readonly Radar radar;
    private readonly PluginCommands pluginCommands;
    private readonly MainUi mainUi;
    private readonly MobEditUi mobEditUi;
    private readonly LocalMobsUi localMobsUi;
    private readonly TypeConfigurator typeConfiguratorUi;
    private readonly CustomizedEntitiesUI customizedEntitiesUi;
    private readonly RadarModules radarModules;
    private readonly IFramework framework;
    private readonly DalamudPluginInterface pluginInterface;
    
    public RadarPlugin(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IObjectTable objectTable,
        ICondition condition,
        IClientState clientState,
        IGameGui gameGui,
        IPluginLog pluginLog,
        IChatGui chatGui,
        IDataManager dataManager,
        IFramework framework)
    {
        Plugin = this;
        this.framework = framework;
        this.pluginInterface = pluginInterface;
        // Services and DI
        Configuration = new Configuration.Configuration(this.pluginInterface, pluginLog);
        radarModules = new RadarModules(condition, clientState, Configuration, dataManager, pluginInterface);

        // UI
        typeConfiguratorUi = new TypeConfigurator(this.pluginInterface, Configuration);
        mobEditUi = new MobEditUi(this.pluginInterface, Configuration, typeConfiguratorUi, radarModules);
        localMobsUi = new LocalMobsUi(this.pluginInterface, Configuration, objectTable, mobEditUi, pluginLog, radarModules);
        customizedEntitiesUi = new CustomizedEntitiesUI(this.pluginInterface, Configuration, pluginLog, typeConfiguratorUi);
        mainUi = new MainUi(this.pluginInterface, Configuration, localMobsUi, clientState, typeConfiguratorUi, customizedEntitiesUi, pluginLog, radarModules);

        // Command manager
        pluginCommands = new PluginCommands(commandManager, mainUi, Configuration, chatGui);
        radar = new Radar(this.pluginInterface, Configuration, objectTable, condition, clientState, gameGui, pluginLog, radarModules);

        this.framework.Update += radarModules.StartTick;
        this.pluginInterface.UiBuilder.Draw += radarModules.EndTick;
    }

    public void Dispose()
    {
        // UI
        mainUi.Dispose();
        localMobsUi.Dispose();
        mobEditUi.Dispose();
        
        // Customer services
        pluginCommands.Dispose();
        radar.Dispose();
        this.framework.Update -= radarModules.StartTick;
        this.pluginInterface.UiBuilder.Draw -= radarModules.EndTick;
    }
}