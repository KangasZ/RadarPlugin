using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using RadarPlugin.Enums;

namespace RadarPlugin;

[Serializable]
public class Configuration
{
    public class OffScreenObjectsOptions
    {
        public float Thickness = 2.2f;
        public float DistanceFromEdge = 15f;
        public float Size = 6.0f;
    }

    public class DeepDungeonMobTypeColorOptions
    {
        public uint Default = UtilInfo.White;
        public uint SpecialUndead = UtilInfo.Yellow;
        public uint Auspice = UtilInfo.Green;
        public uint EasyMobs = UtilInfo.LightBlue;
        public uint Traps = UtilInfo.Orange;
        public uint Return = UtilInfo.Blue;
        public uint Passage = UtilInfo.Blue;
        public uint GoldChest = UtilInfo.Gold;
        public uint SilverChest = UtilInfo.Silver;
        public uint BronzeChest = UtilInfo.Bronze;
        public uint Mimic = UtilInfo.Red;
        public uint AccursedHoard = UtilInfo.Turquoise;
    }

    public class AggroRadiusOptions
    {
        public uint FrontColor = UtilInfo.Red;
        public uint RearColor = UtilInfo.Green;
        public uint RightSideColor = UtilInfo.Yellow;
        public uint LeftSideColor = UtilInfo.Yellow;
        public uint FrontConeColor = UtilInfo.Red;
        public uint CircleOpacity = 0xBEFFFFFF;
        public uint FrontConeOpacity = 0x30FFFFFF;
    }

    public class ESPOption
    {
        public ESPOption()
        {
        }
        // Copy Constructor
        public ESPOption(ESPOption espOption)
        {
            DisplayType = espOption.DisplayType;
            DotSize = espOption.DotSize;
            ColorU = espOption.ColorU;
            ShowAggroCircle = espOption.ShowAggroCircle;
            ShowAggroCircleInCombat = espOption.ShowAggroCircleInCombat;
            ShowFC = espOption.ShowFC;
            DrawDistance = espOption.DrawDistance;
        }

        public DisplayTypes DisplayType = DisplayTypes.NameOnly;
        public float DotSize = 2.2f;
        public uint ColorU = 0xffffffff;
        public bool ShowAggroCircle = false;
        public bool ShowAggroCircleInCombat = false;
        public bool ShowFC = false; // Unused
        public bool DrawDistance = false;
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool ShowBaDdObjects { get; set; } = true;
        public bool ShowLoot { get; set; } = false;
        public bool DebugMode { get; set; } = false;
        public bool ShowPlayers { get; set; } = false;
        public bool ShowEnemies { get; set; } = true;
        public bool ShowEvents { get; set; } = false;
        public bool ShowCompanion { get; set; } = false;
        public bool ShowEventNpc { get; set; } = false;
        public bool ShowAreaObjects { get; set; } = false;
        public bool ShowAetherytes { get; set; } = false;
        public bool ShowNameless { get; set; } = false;
        public bool ShowOnlyVisible { get; set; } = true;
        public bool ShowYOU { get; set; } = false;
        public bool ShowOffScreen { get; set; } = true;
        public OffScreenObjectsOptions OffScreenObjectsOptions { get; set; } = new();
        public DeepDungeonMobTypeColorOptions DeepDungeonMobTypeColorOptions { get; set; } = new();
        public AggroRadiusOptions AggroRadiusOptions { get; set; } = new();
        public ESPOption NpcOption { get; set; } = new(mobOptDefault);
        public ESPOption PlayerOption { get; set; } = new(playerOptDefault);
        public ESPOption ObjectOption { get; set; } = new(objectOptDefault);
        public HashSet<uint> DataIdIgnoreList { get; set; } = new HashSet<uint>();
        public Dictionary<uint, uint> ColorOverride { get; set; } = new Dictionary<uint, uint>();
    }

    public Config cfg;

    [NonSerialized] private DalamudPluginInterface pluginInterface;

    [NonSerialized] private static readonly ESPOption playerOptDefault = new ESPOption
    {
        ColorU = 0xffff00ff,
        DisplayType = DisplayTypes.DotAndName,
        DotSize = 2.2f,
        ShowAggroCircle = false,
        ShowAggroCircleInCombat = false,
        ShowFC = false,
        DrawDistance = false
    };

    [NonSerialized] private static readonly ESPOption objectOptDefault = new ESPOption
    {
        ColorU = 0xffFFFF00,
        DisplayType = DisplayTypes.NameOnly,
        DotSize = 2.2f,
        ShowAggroCircle = false,
        ShowAggroCircleInCombat = false,
        ShowFC = false,
        DrawDistance = false
    };

    [NonSerialized] private static readonly ESPOption mobOptDefault = new ESPOption
    {
        ColorU = 0xffffffff,
        DisplayType = DisplayTypes.HealthBarAndValueAndName,
        DotSize = 2.2f,
        ShowAggroCircle = false,
        ShowAggroCircleInCombat = false,
        ShowFC = false,
        DrawDistance = false
    };

    public Configuration(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        cfg = this.pluginInterface.GetPluginConfig() as Config ?? new Config();
    }

    public void Save()
    {
        pluginInterface.SavePluginConfig(cfg);
    }
}