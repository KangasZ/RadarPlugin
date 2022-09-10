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
        public float Seconds { get; set; } = 30f;
        public bool Instance { get; set; } = false;
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
    public float Seconds {
        get => cfg.Seconds;
        set => cfg.Seconds = value;
    }
    public bool Instance {
        get => cfg.Instance;
        set => cfg.Instance = value;
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