using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Logging;
using ImGuiNET;
using Newtonsoft.Json;
using RadarPlugin.Enums;

namespace RadarPlugin;

[Serializable]
public class Configuration
{
    public class FontSettings
    {
        public bool UseCustomFont = true;
        public bool UseAxisFont = true;
        public float FontSize = ImGui.GetFontSize();
    }

    public class LocalMobsUISettings
    {
        public bool Duplicates = false;
        public bool ShowPlayers = false;
        public bool ShowNpcs = true;
    }
    
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

    public class DeepDungeonOptions
    {
        public ESPOption SpecialUndeadOption { get; set; } = new(mobOptDefault) { ColorU = UtilInfo.Yellow };
        public ESPOption AuspiceOption { get; set; } = new(mobOptDefault) { ColorU = UtilInfo.Green };
        public ESPOption EasyMobOption { get; set; } = new(mobOptDefault) { ColorU = UtilInfo.LightBlue };
        public ESPOption TrapOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Orange };
        public ESPOption ReturnOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Blue };
        public ESPOption PassageOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Blue };
        public ESPOption GoldChestOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Gold };
        public ESPOption SilverChestOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Silver };
        public ESPOption BronzeChestOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Bronze };
        public ESPOption MimicOption { get; set; } = new(mobOptDefault) { ColorU = UtilInfo.Red };
        public ESPOption AccursedHoardOption { get; set; } = new(objectOptDefault) { ColorU = UtilInfo.Turquoise };
        public ESPOption DefaultEnemyOption { get; set; } = new(mobOptDefault) { ColorU = UtilInfo.White };
    }

    public class AggroRadiusOptions
    {
        public bool ShowAggroCircle = false;
        public bool ShowAggroCircleInCombat = false;
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
            ColorU = espOption.ColorU;
            ShowFC = espOption.ShowFC;
            DrawDistance = espOption.DrawDistance;
        }

        public bool Enabled = true;
        public DisplayTypes DisplayType = DisplayTypes.NameOnly;
        public uint ColorU = 0xffffffff;
        public bool ShowFC = false; // Unused
        public bool DrawDistance = false;
        public bool DotSizeOverride = false;
        public float DotSize = UtilInfo.DefaultDotSize;
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public string ConfigName = "default";
        public bool Enabled = true;
        public bool UseBackgroundDrawList = false;
        public bool ShowBaDdObjects = true;
        public bool ShowLoot = false;
        public bool DebugMode = false;
        public bool ShowPlayers = false;
        public bool ShowEnemies = true;
        public bool ShowEvents = false;
        public bool ShowCompanion = false;
        public bool ShowEventNpc= false;
        public bool ShowAreaObjects = false;
        public bool ShowAetherytes = false;
        public bool ShowCardStand = false;
        public bool ShowGatheringPoint = false;
        public bool ShowMountType = false;
        public bool ShowRetainer = false;
        public bool ShowHousing = false;
        public bool ShowCutscene = false;
        public bool ShowNameless = false;
        public bool ShowOnlyVisible = true;
        public bool ShowOffScreen = true;
        public OffScreenObjectsOptions OffScreenObjectsOptions { get; set; } = new();
        public DeepDungeonOptions DeepDungeonOptions { get; set; } = new();
        public AggroRadiusOptions AggroRadiusOptions { get; set; } = new();
        public ESPOption NpcOption { get; set; } = new(mobOptDefault) { Enabled = true };
        public ESPOption PlayerOption { get; set; } = new(playerOptDefault) { Enabled = true };
        public ESPOption YourPlayerOption { get; set; } = new(playerOptDefault) { Enabled = true, ColorU = UtilInfo.Turquoise};
        public ESPOption FriendOption { get; set; } = new(playerOptDefault) { Enabled = true, ColorU = UtilInfo.Orange};
        public ESPOption AllianceOption { get; set; } = new(playerOptDefault) { Enabled = true, ColorU = UtilInfo.Gold};
        public ESPOption PartyOption { get; set; } = new(playerOptDefault) { Enabled = true, ColorU = UtilInfo.Turquoise};
        public ESPOption TreasureOption { get; set; } = new(objectOptDefault) { Enabled = true };
        public ESPOption CompanionOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption AreaOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption AetheryteOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption EventNpcOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption EventObjOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption GatheringPointOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption MountOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption RetainerOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption HousingOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption CutsceneOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption CardStandOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption OrnamentOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public HashSet<uint> DataIdIgnoreList { get; set; } = new HashSet<uint>();
        public Dictionary<uint, uint> ColorOverride { get; set; } = new Dictionary<uint, uint>();
        public HitboxOptions HitboxOptions { get; set; } = new();
        public LocalMobsUISettings LocalMobsUiSettings { get; set; } = new();
        public float DotSize = UtilInfo.DefaultDotSize;
        public bool SeparateAlliance = false;
        public bool SeparateYourPlayer = false;
        public bool SeparateParty = false;
        public bool SeparateFriends = false;
        public FontSettings FontSettings { get; set; } = new();
    }

    public Config cfg;

    [NonSerialized] private DalamudPluginInterface pluginInterface;

    [NonSerialized] private static readonly ESPOption playerOptDefault = new ESPOption
    {
        Enabled = true,
        ColorU = 0xffff00ff,
        DisplayType = DisplayTypes.DotAndName,
        ShowFC = false,
        DrawDistance = false
    };

    [NonSerialized] private static readonly ESPOption objectOptDefault = new ESPOption
    {
        Enabled = true,
        ColorU = 0xffFFFF00,
        DisplayType = DisplayTypes.NameOnly,
        ShowFC = false,
        DrawDistance = false
    };

    [NonSerialized] private static readonly ESPOption mobOptDefault = new ESPOption
    {
        Enabled = true,
        ColorU = 0xffffffff,
        DisplayType = DisplayTypes.HealthBarAndValueAndName,
        ShowFC = false,
        DrawDistance = false
    };

    [NonSerialized] public string[] configs;

    [NonSerialized] public int selectedConfig = 0;

    public Configuration(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        cfg = this.pluginInterface.GetPluginConfig() as Config ?? new Config();

        // Migrate version 0 to 1
        if (cfg.Version == 0)
        {
            cfg.Version = 1;
            cfg.NpcOption.Enabled = cfg.ShowEnemies;
            cfg.PlayerOption.Enabled = cfg.ShowPlayers;
            cfg.TreasureOption.Enabled = cfg.ShowLoot;
            cfg.CompanionOption.Enabled = cfg.ShowCompanion;
            cfg.AreaOption.Enabled = cfg.ShowAreaObjects;
            cfg.AetheryteOption.Enabled = cfg.ShowAetherytes;
            cfg.EventNpcOption.Enabled = cfg.ShowEventNpc;
            cfg.EventObjOption.Enabled = cfg.ShowEvents;
            cfg.GatheringPointOption.Enabled = cfg.ShowGatheringPoint;
            cfg.MountOption.Enabled = cfg.ShowMountType;
            cfg.RetainerOption.Enabled = cfg.ShowRetainer;
            cfg.HousingOption.Enabled = cfg.ShowHousing;
            cfg.CutsceneOption.Enabled = cfg.ShowCutscene;
            cfg.CardStandOption.Enabled = cfg.ShowCardStand;
        }

        var configDirectory = this.pluginInterface.ConfigDirectory;
        if (!configDirectory.Exists)
        {
            configDirectory.Create();
        }

        UpdateConfigs();
    }

    public void SaveCurrentConfig()
    {
        PluginLog.Debug($"Saving config {cfg.ConfigName}");
        SavePluginConfig(cfg, cfg.ConfigName);
        UpdateConfigs();
    }
    
    public bool LoadConfig(string configName)
    {
        PluginLog.Debug($"Loading config {configName}");
        SavePluginConfig(cfg, cfg.ConfigName);
        UpdateConfigs();
        var tempConfig = Load(configName);
        if (tempConfig != null)
        {
            this.cfg = tempConfig;
            Save();
            return true;
        }
        PluginLog.Error("Config was NOT loaded!");
        return false;
    }
    
    public void Save()
    {
        pluginInterface.SavePluginConfig(cfg);
    }
    
    public void UpdateConfigs() {
        configs = this.pluginInterface.ConfigDirectory.GetFiles().Select(x => x.Name).ToArray();
        if (selectedConfig >= configs.Length)
        {
            selectedConfig = 0;
        }
    }

    public void DeleteConfig(string configName)
    {
        PluginLog.Debug($"Deleting config {configName}");
        var path = this.pluginInterface.ConfigDirectory.FullName + "/" + configName;
        FileInfo configFile = new FileInfo(path);
        if (configFile.Exists)
        {
            configFile.Delete();
        }
        UpdateConfigs();
    }
    
    private Config? Load(string configName)
    {

        var path = this.pluginInterface.ConfigDirectory.FullName + "/" + configName;
        FileInfo configFile = new FileInfo(path);
        PluginLog.Debug(configFile.FullName);
        return !configFile.Exists ? null : DeserializeConfig(File.ReadAllText(configFile.FullName));
    }

    internal void SavePluginConfig(Config? currentConfig, string configName)
    {
        if (currentConfig == null)
            return;
        var path = this.pluginInterface.ConfigDirectory.FullName + "/" + configName + ".json";
        this.Save(currentConfig, path);
    }
    internal void Save(Config config, string path) => 
        this.WriteAllTextSafe(path , this.SerializeConfig(config));

    internal string SerializeConfig(Config config) => JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings()
    {
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        TypeNameHandling = TypeNameHandling.Objects
    });
    
    internal void WriteAllTextSafe(string path, string text)
    {
        string str = path + ".tmp";
        if (File.Exists(str))
            File.Delete(str);
        File.WriteAllText(str, text);
        File.Move(str, path, true);
    }
    
    internal static Config? DeserializeConfig(string data) => JsonConvert.DeserializeObject<Config>(data, new JsonSerializerSettings()
    {
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        TypeNameHandling = TypeNameHandling.None
    });
    
}