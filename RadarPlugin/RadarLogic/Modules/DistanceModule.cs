using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace RadarPlugin.RadarLogic.Modules;

public class DistanceModule : IModuleInterface
{
    private Dictionary<uint, float> distanceDictionary = new();

    private void ResetDistance()
    {
        this.distanceDictionary = new Dictionary<uint, float>();
    }
    
    public float GetDistanceFromPlayer(GameObject player, GameObject object2)
    {
        if (distanceDictionary.TryGetValue(object2.ObjectId, out var value))
        {
            return value;
        }

        var distance = object2.Position.Distance2D(player.Position);
        distanceDictionary[object2.ObjectId] = distance;
        return distance;
    }

    public void StartTick()
    {
        //nothing
    }

    public void EndTick()
    {
        ResetDistance();
    }
    
    public void Dispose()
    {
        distanceDictionary.Clear();
    }
}