using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

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

    }

    private Config cfg;
    // the below exist just to make saving less cumbersome

    public int Version {
        get => cfg.Version;
        set => cfg.Version = value;
    }
    public bool Enabled {
        get => cfg.Enabled;
        set => cfg.Enabled = value;
    }
    public bool ObjectShow {
        get => cfg.ObjectShow;
        set => cfg.ObjectShow = value;
    }
    
    public bool DebugMode {
        get => cfg.DebugMode;
        set => cfg.DebugMode = value;
    }
    
    public bool ShowPlayers {
        get => cfg.ShowPlayers;
        set => cfg.ShowPlayers = value;
    }
    
    [NonSerialized]
    private DalamudPluginInterface pluginInterface;

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