using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace RadarPlugin.RadarLogic.Modules;

public class RankModule : IModuleInterface
{
    private readonly IDataManager dataManager;
    private readonly Dictionary<uint, byte> RankDictionary = new();
    private readonly IPluginLog pluginLog;
    
    public RankModule(IDataManager dataManager, IPluginLog pluginLog)
    {
        this.pluginLog = pluginLog;
        this.dataManager = dataManager;

        try
        {
            var excelBnpcs = this.dataManager.Excel.GetSheet<BNpcBase>();
            if (excelBnpcs != null)
            {
                RankDictionary = excelBnpcs.ToDictionary(x => x.RowId, x => x.Rank);
            }
        } catch (Exception e)
        {
            pluginLog.Error(e, "Failed to load RankModule");
        }

    }

    public bool TryGetRank(uint rowId, out byte rank)
    {
        var tryGetValue = RankDictionary.TryGetValue(rowId, out var value);
        rank = value;
        return tryGetValue;
    }
    
    public void Dispose()
    {
        RankDictionary.Clear();
    }

    public void StartTick()
    {
        //Nothing
    }

    public void EndTick()
    {
        //Also Nothing
    }
}