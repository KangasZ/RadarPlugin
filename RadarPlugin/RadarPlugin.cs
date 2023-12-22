﻿using Dalamud.Plugin;
using Dalamud.Plugin.Services;
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
    private readonly CustomizedEntitiesUI customizedEntitiesUi;

    public RadarPlugin(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IObjectTable objectTable,
        ICondition condition,
        IClientState clientState,
        IGameGui gameGui,
        IPluginLog pluginLog,
        IChatGui chatGui,
        IDataManager dataManager)
    {
        Plugin = this;
        // Services and DI
        Configuration = new Configuration(pluginInterface, pluginLog);
        radarHelpers = new RadarHelpers(Configuration, clientState, condition, dataManager);

        // UI
        typeConfiguratorUi = new TypeConfigurator(pluginInterface, Configuration, radarHelpers);
        mobEditUi = new MobEditUi(pluginInterface, Configuration, radarHelpers, typeConfiguratorUi);
        localMobsUi = new LocalMobsUi(pluginInterface, Configuration, objectTable, mobEditUi, radarHelpers, pluginLog);
        customizedEntitiesUi = new CustomizedEntitiesUI(pluginInterface, Configuration, radarHelpers, pluginLog, typeConfiguratorUi);
        mainUi = new MainUi(pluginInterface, Configuration, localMobsUi, clientState, radarHelpers, typeConfiguratorUi, customizedEntitiesUi, pluginLog);

        // Command manager
        pluginCommands = new PluginCommands(commandManager, mainUi, Configuration, chatGui);
        radarLogic = new RadarLogic(pluginInterface, Configuration, objectTable, condition, clientState, gameGui, radarHelpers, pluginLog);
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