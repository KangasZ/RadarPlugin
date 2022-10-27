using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace RadarPlugin;

[Serializable]
public class Configuration
{
    private class Config : IPluginConfiguration
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
        public HashSet<uint> DataIdIgnoreList { get; set; } = new HashSet<uint>();
    }
    private Config cfg;

    public int Version
    {
        get => cfg.Version;
        set => cfg.Version = value;
    }

    public bool Enabled
    {
        get => cfg.Enabled;
        set => cfg.Enabled = value;
    }

    public bool ShowLoot
    {
        get => cfg.ShowLoot;
        set => cfg.ShowLoot = value;
    }

    public bool DebugMode
    {
        get => cfg.DebugMode;
        set => cfg.DebugMode = value;
    }

    public bool ShowPlayers
    {
        get => cfg.ShowPlayers;
        set => cfg.ShowPlayers = value;
    }

    public bool ShowEnemies
    {
        get => cfg.ShowEnemies;
        set => cfg.ShowEnemies = value;
    }
    
    public bool ShowEvents
    {
        get => cfg.ShowEvents;
        set => cfg.ShowEvents = value;
    }
    
    public bool ShowCompanion
    {
        get => cfg.ShowCompanion;
        set => cfg.ShowCompanion = value;
    }
    
    public bool ShowEventNpc
    {
        get => cfg.ShowEventNpc;
        set => cfg.ShowEventNpc = value;
    }
    
    public bool ShowAreaObjects
    {
        get => cfg.ShowAreaObjects;
        set => cfg.ShowAreaObjects = value;
    }
    
    public bool ShowAetherytes
    {
        get => cfg.ShowAetherytes;
        set => cfg.ShowAetherytes = value;
    }
    
    public HashSet<uint> DataIdIgnoreList
    {
        get => cfg.DataIdIgnoreList;
        set => cfg.DataIdIgnoreList = value;
    }
    
    public bool ShowBaDdObjects
    {
        get => cfg.ShowBaDdObjects;
        set => cfg.ShowBaDdObjects = value;
    }
    
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