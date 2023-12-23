using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.Types;
using RadarPlugin.Enums;
using RadarPlugin.Models;

namespace RadarPlugin.RadarLogic.Modules;

public class AggroTypeModule : IModuleInterface
{
    private readonly Dictionary<uint, AggroType> AggroTypeDictionary = new();

    public AggroTypeModule()
    {
        var json = File.ReadAllText("Data/allMobs.json");
        var mobs = JsonSerializer.Deserialize<List<AggroInfo>>(json);
        if (mobs.Count > 0)
        {
            AggroTypeDictionary = mobs.ToDictionary(mob => mob.NameId, mob => mob.AggroType);
        }
    }

    public AggroType GetAggroType(uint nameId)
    {
        if (AggroTypeDictionary.TryGetValue(nameId, out var value))
        {
            return value;
        }

        return AggroType.Default;
    }

    public void StartTick()
    {
        // Do Nothing
    }

    public void EndTick()
    {
        // Also do nothing
    }
    
    public void Dispose()
    {
        AggroTypeDictionary.Clear();
    }
}