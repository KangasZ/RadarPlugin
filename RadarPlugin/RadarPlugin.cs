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
    private readonly RadarDriver radarDriver;
    private readonly PluginCommands pluginCommands;
    private readonly MainUi mainUi;
    private readonly MobEditUi mobEditUi;
    private readonly LocalMobsUi localMobsUi;
    private readonly TypeConfigurator typeConfiguratorUi;
    private readonly CustomizedEntitiesUI customizedEntitiesUi;
    private readonly RadarModules radarModules;
    private readonly IFramework framework;
    private readonly IDalamudPluginInterface pluginInterface;

    public RadarPlugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IObjectTable objectTable,
        ICondition condition,
        IClientState clientState,
        IGameGui gameGui,
        IPluginLog pluginLog,
        IChatGui chatGui,
        IDataManager dataManager,
        IFramework framework,
        IGameInteropProvider gameInteropProvider
    )
    {
        Plugin = this;
        this.framework = framework;
        this.pluginInterface = pluginInterface;
        // Services and DI
        Configuration = new Configuration.Configuration(this.pluginInterface, pluginLog);
        radarModules = new RadarModules(
            condition,
            clientState,
            Configuration,
            dataManager,
            pluginInterface,
            pluginLog
        );

        // UI
        typeConfiguratorUi = new TypeConfigurator(this.pluginInterface, Configuration, pluginLog);
        mobEditUi = new MobEditUi(
            this.pluginInterface,
            Configuration,
            typeConfiguratorUi,
            radarModules,
            clientState
        );
        localMobsUi = new LocalMobsUi(
            this.pluginInterface,
            Configuration,
            objectTable,
            mobEditUi,
            pluginLog,
            radarModules,
            typeConfiguratorUi,
            clientState
        );
        customizedEntitiesUi = new CustomizedEntitiesUI(
            this.pluginInterface,
            Configuration,
            pluginLog,
            typeConfiguratorUi
        );
        mainUi = new MainUi(
            this.pluginInterface,
            Configuration,
            localMobsUi,
            clientState,
            typeConfiguratorUi,
            customizedEntitiesUi,
            pluginLog,
            radarModules
        );

        // Command manager
        pluginCommands = new PluginCommands(commandManager, mainUi, Configuration, chatGui);
        radarDriver = new RadarDriver(
            this.pluginInterface,
            Configuration,
            objectTable,
            condition,
            clientState,
            gameGui,
            pluginLog,
            radarModules,
            gameInteropProvider
        );

        this.framework.Update += radarModules.StartTick;
        this.pluginInterface.UiBuilder.Draw += radarModules.EndTick;
    }

    public void Dispose()
    {
        Configuration.Save();
        // UI
        mainUi.Dispose();
        localMobsUi.Dispose();
        mobEditUi.Dispose();

        // Customer services
        pluginCommands.Dispose();
        radarDriver.Dispose();
        this.framework.Update -= radarModules.StartTick;
        this.pluginInterface.UiBuilder.Draw -= radarModules.EndTick;
    }
}
