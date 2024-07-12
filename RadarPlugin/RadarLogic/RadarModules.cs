using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RadarPlugin.RadarLogic.Modules;

namespace RadarPlugin.RadarLogic;

public class RadarModules : IModuleInterface
{
    public AggroTypeModule aggroTypeModule;
    public DistanceModule distanceModule;
    public MobLastMovement moduleMobLastMovement;
    public RadarConfigurationModule radarConfigurationModule;
    public RankModule rankModule;
    public ZoneTypeModule zoneTypeModule;

    public RadarModules(ICondition conditionInterface, IClientState clientState, Configuration.Configuration configInterface, IDataManager dataManager, IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
    {
        aggroTypeModule = new AggroTypeModule(pluginInterface);
        distanceModule = new DistanceModule();
        moduleMobLastMovement = new MobLastMovement();
        rankModule = new RankModule(dataManager);
        zoneTypeModule = new ZoneTypeModule(conditionInterface, clientState);
        radarConfigurationModule = new RadarConfigurationModule(clientState, configInterface, zoneTypeModule, rankModule, distanceModule, moduleMobLastMovement, pluginLog);
    }

    public void Dispose()
    {
        aggroTypeModule.Dispose();
        distanceModule.Dispose();
        moduleMobLastMovement.Dispose();
        rankModule.Dispose();
        zoneTypeModule.Dispose();
        radarConfigurationModule.Dispose();
    }

    public void StartTick(IFramework _) => StartTick();
    
    public void StartTick()
    {
        aggroTypeModule.StartTick();
        distanceModule.StartTick();
        moduleMobLastMovement.StartTick();
        rankModule.StartTick();
        zoneTypeModule.StartTick();
        radarConfigurationModule.StartTick();
    }

    public void EndTick()
    {
        aggroTypeModule.EndTick();
        distanceModule.EndTick();
        moduleMobLastMovement.EndTick();
        rankModule.EndTick();
        zoneTypeModule.EndTick();
        radarConfigurationModule.EndTick();
    }
}