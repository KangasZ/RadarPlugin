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
        public bool ObjectShow { get; set; } = false;
        public bool DebugMode { get; set; } = false;
        public bool ShowPlayers { get; set; } = false;
        public bool ShowEnemies { get; set; } = true;
        public bool ShowEvents { get; set; } = false;
        public bool ShowNpc { get; set; } = false;
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

    public bool ShowObjects
    {
        get => cfg.ObjectShow;
        set => cfg.ObjectShow = value;
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
    
    public bool ShowNpc
    {
        get => cfg.ShowNpc;
        set => cfg.ShowNpc = value;
    }
    
    public HashSet<uint> DataIdIgnoreList
    {
        get => cfg.DataIdIgnoreList;
        set => cfg.DataIdIgnoreList = value;

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