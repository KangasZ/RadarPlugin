using System;
using RadarPlugin.Enums;

namespace RadarPlugin.Configuration.Models.ESPOption;

public class ESPOptionMobBased : ESPOption
{
    public ESPOptionMobBased() { }

    public ESPOptionMobBased(Models.ESPOption.ESPOption espOption)
    {
        Enabled = espOption.Enabled;
        DisplayType = espOption.DisplayType;
        DisplayTypeFlags = espOption.DisplayTypeFlags;
        DisplayTypeFlags2D = espOption.DisplayTypeFlags2D;
        ColorU = espOption.ColorU;
        DrawDistance = espOption.DrawDistance;
        AppendLevelToName = espOption.AppendLevelToName;
    }

    public ESPOptionMobBased(
        Models.ESPOption.ESPOption espOption,
        string name,
        ulong id,
        MobType mobType = MobType.Object,
        ulong tertiaryId = 0
    )
    {
        Name = name;
        Enabled = espOption.Enabled;
        DisplayType = espOption.DisplayType;
        DisplayTypeFlags = espOption.DisplayTypeFlags;
        DisplayTypeFlags2D = espOption.DisplayTypeFlags2D;
        ColorU = espOption.ColorU;
        DrawDistance = espOption.DrawDistance;
        AppendLevelToName = espOption.AppendLevelToName;
        MobTypeValue = mobType;
        this.Id = id;
        this.TertiaryId = tertiaryId;
    }

    public ulong TertiaryId = 0;
    public ulong Id = 0;
    public DateTime UtcLastSeenTime = DateTime.UtcNow;
    public string LastSeenName = string.Empty;
    public MobType MobTypeValue = MobType.Object;
    public string Name = string.Empty;
}
