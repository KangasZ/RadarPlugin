using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

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
        public Dictionary<uint, Vector4> CustomColorOverride { get; set; } = new Dictionary<uint, Vector4>();
    }

    public Config cfg;

    [NonSerialized] private DalamudPluginInterface pluginInterface;

    public Configuration(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        cfg = this.pluginInterface.GetPluginConfig() as Config ?? new Config();
    }


    public Vector4 GetColor(GameObject gameObject)
    {
        Vector4 color;
        if (cfg.CustomColorOverride.ContainsKey(gameObject.DataId))
        {
            color = cfg.CustomColorOverride[gameObject.DataId];
        }
        else
        {
            switch (gameObject.ObjectKind)
            {
                case ObjectKind.Player:
                    color = cfg.PlayerOption.Color;
                    break;
                case ObjectKind.BattleNpc:
                    color = cfg.NpcOption.Color;
                    break;
                case ObjectKind.None:
                case ObjectKind.EventNpc:
                case ObjectKind.Treasure:
                case ObjectKind.Aetheryte:
                case ObjectKind.GatheringPoint:
                case ObjectKind.EventObj:
                case ObjectKind.MountType:
                case ObjectKind.Companion:
                case ObjectKind.Retainer:
                case ObjectKind.Area:
                case ObjectKind.Housing:
                case ObjectKind.Cutscene:
                case ObjectKind.CardStand:
                default:
                    color = cfg.ObjectOption.Color;
                    break;
            }
        }

        return color;
    }
    
    public void Save()
    {
        pluginInterface.SavePluginConfig(cfg);
    }
}