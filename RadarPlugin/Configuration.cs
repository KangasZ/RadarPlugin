using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using RadarPlugin.Enums;

namespace RadarPlugin;

[Serializable]
public class Configuration
{
    public class HitboxOptions
    {
        public bool HitboxEnabled = false;
        public uint HitboxColor = UtilInfo.Turquoise;
    }
    
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

    public class DeepDungeonOptions
    {
        public ESPOption SpecialUndeadOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Yellow};
        public ESPOption AuspiceOption { get; set; } = new(objectOptDefault)  { ColorU = UtilInfo.Green};
        public ESPOption EasyMobOption { get; set; } = new(objectOptDefault)  { ColorU = UtilInfo.LightBlue};
        public ESPOption TrapOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Orange};
        public ESPOption ReturnOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Blue };
        public ESPOption PassageOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Blue};
        public ESPOption GoldChestOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Gold};
        public ESPOption SilverChestOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Silver};
        public ESPOption BronzeChestOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Bronze};
        public ESPOption MimicOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Red};
        public ESPOption AccursedHoardOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Turquoise};
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
            Enabled = espOption.Enabled;
            DisplayType = espOption.DisplayType;
            DotSize = espOption.DotSize;
            ColorU = espOption.ColorU;
            ShowAggroCircle = espOption.ShowAggroCircle;
            ShowAggroCircleInCombat = espOption.ShowAggroCircleInCombat;
            ShowFC = espOption.ShowFC;
            DrawDistance = espOption.DrawDistance;
        }

        public bool Enabled = true;
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
        public bool ShowCardStand { get; set; } = false;
        public bool ShowGatheringPoint { get; set; } = false;
        public bool ShowMountType { get; set; } = false;
        public bool ShowRetainer { get; set; } = false;
        public bool ShowHousing { get; set; } = false;
        public bool ShowCutscene { get; set; } = false;
        public bool ShowNameless { get; set; } = false;
        public bool ShowOnlyVisible { get; set; } = true;
        public bool ShowYOU { get; set; } = false;
        public bool ShowOffScreen { get; set; } = true;
        public OffScreenObjectsOptions OffScreenObjectsOptions { get; set; } = new();
        public DeepDungeonOptions DeepDungeonOptions { get; set; } = new();
        public AggroRadiusOptions AggroRadiusOptions { get; set; } = new();
        public ESPOption NpcOption { get; set; } = new(mobOptDefault);
        public ESPOption PlayerOption { get; set; } = new(playerOptDefault);
        public ESPOption ObjectOption { get; set; } = new(objectOptDefault);
        public ESPOption TreasureOption { get; set; } = new(objectOptDefault);
        public ESPOption CompanionOption { get; set; } = new(objectOptDefault);
        public ESPOption AreaOption { get; set; } = new(objectOptDefault);
        public ESPOption AetheryteOption { get; set; } = new(objectOptDefault);
        public ESPOption EventNpcOption { get; set; } = new(objectOptDefault);
        public ESPOption EventObjOption { get; set; } = new(objectOptDefault);
        public ESPOption GatheringPointOption { get; set; } = new(objectOptDefault);
        public ESPOption MountOption { get; set; } = new(objectOptDefault);
        public ESPOption RetainerOption { get; set; } = new(objectOptDefault);
        public ESPOption HousingOption { get; set; } = new(objectOptDefault);
        public ESPOption CutsceneOption { get; set; } = new(objectOptDefault);
        public ESPOption CardStandOption { get; set; } = new(objectOptDefault);
        public HashSet<uint> DataIdIgnoreList { get; set; } = new HashSet<uint>();
        public Dictionary<uint, uint> ColorOverride { get; set; } = new Dictionary<uint, uint>();
        public HitboxOptions HitboxOptions { get; set; } = new();
    }

    public Config cfg;

    [NonSerialized] private DalamudPluginInterface pluginInterface;

    [NonSerialized] private static readonly ESPOption playerOptDefault = new ESPOption
    {
        Enabled = true,
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
        Enabled = true,
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
        Enabled = true,
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