using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using RadarPlugin.Constants;
using RadarPlugin.Enums;

namespace RadarPlugin.RadarLogic.Modules;

public class ZoneTypeModule : IModuleInterface
{
    private ICondition conditionInterface;
    private readonly IClientState clientState;
    private LocationKind currentLocation = LocationKind.Overworld;

    public ZoneTypeModule(ICondition conditionInterface, IClientState clientState)
    {
        this.conditionInterface = conditionInterface;
        this.clientState = clientState;
    }

    public LocationKind GetLocationType()
    {
        return this.currentLocation;
    }

    public void Dispose()
    {
        //nothing
    }

    public void StartTick()
    {
        if (
            MobConstants.DeepDungeonMapIds.Contains(this.clientState.TerritoryType)
            || this.conditionInterface[ConditionFlag.InDeepDungeon]
        )
        {
            currentLocation = LocationKind.DeepDungeon;
        }
        else
        {
            currentLocation = LocationKind.Overworld;
        }
    }

    public void EndTick()
    {
        //nothing
    }
}
