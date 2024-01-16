using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace RadarPlugin.RadarLogic.Modules;

public class RankModule : IModuleInterface
{
    private readonly IDataManager dataManager;
    private readonly Dictionary<uint, byte> RankDictionary = new();

    public RankModule(IDataManager dataManager)
    {
        this.dataManager = dataManager;

        var excelBnpcs = this.dataManager.GetExcelSheet<BNpcBase>();
        if (excelBnpcs != null)
        {
            RankDictionary = excelBnpcs.ToDictionary(x => x.RowId, x => x.Rank);
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