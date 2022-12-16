using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace RadarPlugin;

[Serializable]
public class Configuration
{
    public class ESPOption
    {
        public bool ShowName = true;
        public bool ShowDot = false;
    }

    public class ObjectOption : ESPOption
    {
        public Vector4 Color = new(0x00, 0x7e, 0x7e, 0xFF);
    }

    public class NpcOption : ESPOption
    {
        public bool ShowHealthBar = true;
        public bool ShowHealthValue = true;
        public bool ShowAggroCircle = false;
        public Vector4 Color = new(0xff, 0xff, 0xff, 0xff);
    }

    public class PlayerOption : NpcOption
    {
        public bool ShowFC = false; // Unused
        public new bool ShowHealthBar = false;
        public new bool ShowHealthValue = false;
        public new bool ShowDot = true;
        public new Vector4 Color = new(0x99, 0x00, 0x99, 0xFF);
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
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
        public bool ShowOnlyVisible { get; set; } = true;
        public NpcOption NpcOption { get; set; } = new();
        public PlayerOption PlayerOption { get; set; } = new();
        public ObjectOption ObjectOption { get; set; } = new();
        public HashSet<uint> DataIdIgnoreList { get; set; } = new HashSet<uint>();
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