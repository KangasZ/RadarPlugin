using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
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
        public DisplayTypes DisplayType = DisplayTypes.NameOnly;
        public float DotSize = 2.2f;
        public bool DrawDistance = false;
    }

    public class ObjectOption : ESPOption
    {
        public uint ColorU = 0xffFFFF00; //new(0x00, 0x7e, 0x7e, 0xFF);
    }

    public class NpcOption : ESPOption
    {
        public new DisplayTypes DisplayType = DisplayTypes.HealthBarAndValueAndName;
        public bool ShowAggroCircle = false;
        public bool ShowAggroCircleInCombat = false;
        public uint ColorU = 0xffffffff; //new(0xff, 0xff, 0xff, 0xff);
    }

    public class PlayerOption : NpcOption
    {
        public bool ShowFC = false; // Unused
        public new DisplayTypes DisplayType = DisplayTypes.DotAndName;
        public new uint ColorU = 0xffff00ff; //new(0x99, 0x00, 0x99, 0xFF);
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
        public NpcOption NpcOption { get; set; } = new();
        public PlayerOption PlayerOption { get; set; } = new();
        public ObjectOption ObjectOption { get; set; } = new();
        public HashSet<uint> DataIdIgnoreList { get; set; } = new HashSet<uint>();
        public Dictionary<uint, uint> ColorOverride { get; set; } = new Dictionary<uint, uint>();
    }

    public Config cfg;

    [NonSerialized] private DalamudPluginInterface pluginInterface;

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
