using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using RadarPlugin.Configuration.Models;
using RadarPlugin.Configuration.Models.ESPOption;
using RadarPlugin.Configuration.Models.FeatureSettings;
using RadarPlugin.Configuration.Models.UiSettings;
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.Configuration;

[Serializable]
public class Configuration
{
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 6;
        public string ConfigName = "default";
        public uint YourAccountId = 0;
        public bool Enabled = true;
        public bool Radar3DEnabled = true;
        public bool UseBackgroundDrawList = false;
        public bool ShowBaDdObjects = true;
        public bool DebugMode = false;
        public bool RankText = false;
        public bool DebugText = false;
        public bool ShowNameless = false;
        public bool ShowOnlyVisible = true;
        public bool OverrideShowInvisiblePlayerCharacters = true;
        public bool ShowOffScreen = false;
        public OffScreenObjectsOptions OffScreenObjectsOptions { get; set; } = new();
        public DeepDungeonOptions DeepDungeonOptions { get; set; } = new();
        public AggroRadiusOptions AggroRadiusOptions { get; set; } = new();
        public CastBarOptions CastBarOptions { get; set; } = new();
        public ESPOption NpcOption { get; set; } =
            new(DefaultESPOptions.mobOptDefault) { Enabled = true, AppendLevelToName = false };
        public ESPOption PlayerOption { get; set; } = new(DefaultESPOptions.playerOptDefault);
        public ESPOption TreasureOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = true };
        public ESPOption CompanionOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption AreaOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption AetheryteOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption EventNpcOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption EventObjOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption GatheringPointOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption MountOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption RetainerOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption HousingOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption CutsceneOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption CardStandOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public ESPOption OrnamentOption { get; set; } =
            new(DefaultESPOptions.objectOptDefault) { Enabled = false };
        public Dictionary<uint, ESPOptionMobBased> OptionOverride { get; set; } =
            new Dictionary<uint, ESPOptionMobBased>();
        public Dictionary<ulong, ESPOptionMobBased> PlayerOptionOverride { get; set; } =
            new Dictionary<ulong, ESPOptionMobBased>();
        public HitboxOptions HitboxOptions { get; set; } = new();
        public LocalMobsUISettings LocalMobsUiSettings { get; set; } = new();
        public float DotSize = ConfigConstants.DefaultDotSize;
        public float DotSize2D = ConfigConstants.DefaultDotSize;
        public bool UseMaxDistance = false;
        public float MaxDistance = ConfigConstants.DefaultMaxEspDistance;
        public FontSettings FontSettings { get; set; } = new();
        public LevelRendering LevelRendering { get; set; } = new();
        public bool ShowOverworldObjects = true;

        public float EspPadding = ConfigConstants.DefaultEspPadding;

        public SeparatedEspOption SeparatedAlliance = new()
        {
            EspOption = new ESPOption(DefaultESPOptions.playerOptDefault)
            {
                ColorU = ConfigConstants.Gold,
            },
        };

        public SeparatedEspOption SeparatedYourPlayer = new()
        {
            EspOption = new ESPOption(DefaultESPOptions.playerOptDefault)
            {
                ColorU = ConfigConstants.Turquoise,
            },
        };

        public SeparatedEspOption SeparatedParty = new()
        {
            EspOption = new ESPOption(DefaultESPOptions.playerOptDefault)
            {
                ColorU = ConfigConstants.Turquoise,
            },
        };

        public SeparatedEspOption SeparatedFriends = new()
        {
            EspOption = new ESPOption(DefaultESPOptions.playerOptDefault)
            {
                ColorU = ConfigConstants.Orange,
            },
        };

        public SeparatedEspOption SeparatedRankOne = new()
        {
            EspOption = new ESPOption(DefaultESPOptions.mobOptDefault)
            {
                ColorU = ConfigConstants.Gold,
            },
        };

        public SeparatedEspOption SeparatedRankTwoAndSix = new()
        {
            EspOption = new ESPOption(DefaultESPOptions.mobOptDefault)
            {
                ColorU = ConfigConstants.Yellow,
            },
        };

        public bool EXPERIMENTALEnableMobTimerTracking = false;
        public Radar2DConfiguration Radar2DConfiguration = new();
    }

    public Config cfg;

    [NonSerialized]
    private IDalamudPluginInterface pluginInterface;

    [NonSerialized]
    public string[] configs = new[] { "" };

    [NonSerialized]
    public int selectedConfig = 0;

    [NonSerialized]
    private readonly IPluginLog pluginLog;

    public Configuration(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
    {
        this.pluginInterface = pluginInterface;
        cfg = this.pluginInterface.GetPluginConfig() as Config ?? new Config();
        MigrateCfg(ref cfg);

        var configDirectory = this.pluginInterface.ConfigDirectory;
        if (!configDirectory.Exists)
        {
            configDirectory.Create();
        }

        this.pluginLog = pluginLog;
        UpdateConfigs();
    }

    public void Customize(
        IGameObject gameObject,
        bool customizeEnabled,
        ESPOption currentSettings,
        ulong obfuscatedSelfId,
        uint yourBaseId
    )
    {
        var dataId = gameObject.DataId;
        if (gameObject.ObjectKind == ObjectKind.Pc)
        {
            var contentId = gameObject.GetContentId();

            if (customizeEnabled)
            {
                var newSettings = new ESPOptionMobBased(
                    currentSettings,
                    gameObject.Name.TextValue ?? "Oops it broke :(",
                    contentId,
                    MobType.Player,
                    tertiaryId: contentId
                );
                if (cfg.PlayerOptionOverride.ContainsKey(contentId))
                {
                    cfg.PlayerOptionOverride.Remove(contentId);
                }

                cfg.PlayerOptionOverride.Add(contentId, newSettings);
            }
            else
            {
                cfg.PlayerOptionOverride.Remove(contentId);
            }
        }
        else
        {
            if (customizeEnabled)
            {
                var mobtype = gameObject.GetMobType();
                var newSettings = new ESPOptionMobBased(
                    currentSettings,
                    gameObject.Name.TextValue ?? "Unknown",
                    dataId,
                    mobtype
                );
                if (cfg.OptionOverride.ContainsKey(dataId))
                {
                    cfg.OptionOverride.Remove(dataId);
                }

                cfg.OptionOverride.Add(dataId, newSettings);
            }
            else
            {
                cfg.OptionOverride.Remove(dataId);
            }
        }
    }

    private void MigrateCfg(ref Config oldConfig)
    {
        // Migrate version 2 to 4

        if (oldConfig.Version <= 3)
        {
            foreach (var espOptionMobBased in oldConfig.OptionOverride)
            {
                espOptionMobBased.Value.DisplayTypeFlags =
                    espOptionMobBased.Value.DisplayType.ToFlags(
                        espOptionMobBased.Value.DrawDistance
                    );
            }

            oldConfig.SeparatedAlliance.EspOption.DisplayTypeFlags =
                oldConfig.SeparatedAlliance.EspOption.DisplayType.ToFlags(
                    oldConfig.SeparatedAlliance.EspOption.DrawDistance
                );
            oldConfig.SeparatedYourPlayer.EspOption.DisplayTypeFlags =
                oldConfig.SeparatedYourPlayer.EspOption.DisplayType.ToFlags(
                    oldConfig.SeparatedYourPlayer.EspOption.DrawDistance
                );
            oldConfig.SeparatedParty.EspOption.DisplayTypeFlags =
                oldConfig.SeparatedParty.EspOption.DisplayType.ToFlags(
                    oldConfig.SeparatedParty.EspOption.DrawDistance
                );
            oldConfig.SeparatedFriends.EspOption.DisplayTypeFlags =
                oldConfig.SeparatedFriends.EspOption.DisplayType.ToFlags(
                    oldConfig.SeparatedFriends.EspOption.DrawDistance
                );
            oldConfig.SeparatedRankOne.EspOption.DisplayTypeFlags =
                oldConfig.SeparatedRankOne.EspOption.DisplayType.ToFlags(
                    oldConfig.SeparatedRankOne.EspOption.DrawDistance
                );
            oldConfig.SeparatedRankTwoAndSix.EspOption.DisplayTypeFlags =
                oldConfig.SeparatedRankTwoAndSix.EspOption.DisplayType.ToFlags(
                    oldConfig.SeparatedRankTwoAndSix.EspOption.DrawDistance
                );

            oldConfig.NpcOption.DisplayTypeFlags = oldConfig.NpcOption.DisplayType.ToFlags(
                oldConfig.NpcOption.DrawDistance
            );
            oldConfig.PlayerOption.DisplayTypeFlags = oldConfig.PlayerOption.DisplayType.ToFlags(
                oldConfig.PlayerOption.DrawDistance
            );
            oldConfig.TreasureOption.DisplayTypeFlags =
                oldConfig.TreasureOption.DisplayType.ToFlags(oldConfig.TreasureOption.DrawDistance);
            oldConfig.CompanionOption.DisplayTypeFlags =
                oldConfig.CompanionOption.DisplayType.ToFlags(
                    oldConfig.CompanionOption.DrawDistance
                );
            oldConfig.AreaOption.DisplayTypeFlags = oldConfig.AreaOption.DisplayType.ToFlags(
                oldConfig.AreaOption.DrawDistance
            );
            oldConfig.AetheryteOption.DisplayTypeFlags =
                oldConfig.AetheryteOption.DisplayType.ToFlags(
                    oldConfig.AetheryteOption.DrawDistance
                );
            oldConfig.EventNpcOption.DisplayTypeFlags =
                oldConfig.EventNpcOption.DisplayType.ToFlags(oldConfig.EventNpcOption.DrawDistance);
            oldConfig.EventObjOption.DisplayTypeFlags =
                oldConfig.EventObjOption.DisplayType.ToFlags(oldConfig.EventObjOption.DrawDistance);
            oldConfig.GatheringPointOption.DisplayTypeFlags =
                oldConfig.GatheringPointOption.DisplayType.ToFlags(
                    oldConfig.GatheringPointOption.DrawDistance
                );
            oldConfig.MountOption.DisplayTypeFlags = oldConfig.MountOption.DisplayType.ToFlags(
                oldConfig.MountOption.DrawDistance
            );
            oldConfig.RetainerOption.DisplayTypeFlags =
                oldConfig.RetainerOption.DisplayType.ToFlags(oldConfig.RetainerOption.DrawDistance);
            oldConfig.HousingOption.DisplayTypeFlags = oldConfig.HousingOption.DisplayType.ToFlags(
                oldConfig.HousingOption.DrawDistance
            );
            oldConfig.CutsceneOption.DisplayTypeFlags =
                oldConfig.CutsceneOption.DisplayType.ToFlags(oldConfig.CutsceneOption.DrawDistance);
            oldConfig.CardStandOption.DisplayTypeFlags =
                oldConfig.CardStandOption.DisplayType.ToFlags(
                    oldConfig.CardStandOption.DrawDistance
                );
            oldConfig.OrnamentOption.DisplayTypeFlags =
                oldConfig.OrnamentOption.DisplayType.ToFlags(oldConfig.OrnamentOption.DrawDistance);

            oldConfig.DeepDungeonOptions.SpecialUndeadOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.SpecialUndeadOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.SpecialUndeadOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.AuspiceOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.AuspiceOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.AuspiceOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.EasyMobOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.EasyMobOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.EasyMobOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.TrapOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.TrapOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.TrapOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.ReturnOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.ReturnOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.ReturnOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.PassageOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.PassageOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.PassageOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.GoldChestOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.GoldChestOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.GoldChestOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.SilverChestOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.SilverChestOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.SilverChestOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.BronzeChestOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.BronzeChestOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.BronzeChestOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.MimicOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.MimicOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.MimicOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.AccursedHoardOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.AccursedHoardOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.AccursedHoardOption.DrawDistance
                );
            oldConfig.DeepDungeonOptions.DefaultEnemyOption.DisplayTypeFlags =
                oldConfig.DeepDungeonOptions.DefaultEnemyOption.DisplayType.ToFlags(
                    oldConfig.DeepDungeonOptions.DefaultEnemyOption.DrawDistance
                );

            oldConfig.Version = 4;
        }

        if (oldConfig.Version <= 5)
        {
            oldConfig.OptionOverride = oldConfig.OptionOverride.ToDictionary(
                x => x.Key,
                y =>
                {
                    var config = y.Value;
                    config.Id = y.Key;
                    return config;
                }
            );
            oldConfig.PlayerOptionOverride = oldConfig.PlayerOptionOverride.ToDictionary(
                x => x.Key,
                y =>
                {
                    var config = y.Value;
                    config.Id = y.Key;
                    return config;
                }
            );
            oldConfig.Version = 5;
        }

        if (oldConfig.Version <= 6)
        {
            oldConfig.AggroRadiusOptions.FrontColor &= oldConfig.AggroRadiusOptions.CircleOpacity;
            oldConfig.AggroRadiusOptions.LeftSideColor &= oldConfig
                .AggroRadiusOptions
                .CircleOpacity;
            oldConfig.AggroRadiusOptions.RightSideColor &= oldConfig
                .AggroRadiusOptions
                .CircleOpacity;
            oldConfig.AggroRadiusOptions.RearColor &= oldConfig.AggroRadiusOptions.CircleOpacity;
            oldConfig.AggroRadiusOptions.SoundAggroColor &= oldConfig
                .AggroRadiusOptions
                .CircleOpacity;
            oldConfig.AggroRadiusOptions.ProximityAggroColor &= oldConfig
                .AggroRadiusOptions
                .CircleOpacity;
            oldConfig.AggroRadiusOptions.FrontConeColor &= oldConfig
                .AggroRadiusOptions
                .FrontConeOpacity;
            oldConfig.Version = 6;
        }
    }

    public void SaveCurrentConfig()
    {
        pluginLog.Debug($"Saving config {cfg.ConfigName}");
        SavePluginConfig(cfg, cfg.ConfigName);
    }

    public bool LoadConfig(string configName)
    {
        pluginLog.Debug($"Loading config {configName}");
        SavePluginConfig(cfg, cfg.ConfigName);
        UpdateConfigs();
        var tempConfig = Load(configName);
        if (tempConfig != null)
        {
            this.cfg = tempConfig;
            MigrateCfg(ref cfg);
            Save();
            return true;
        }

        pluginLog.Error("Config was NOT loaded!");
        return false;
    }

    public void Save()
    {
        pluginInterface.SavePluginConfig(cfg);
    }

    public void UpdateConfigs()
    {
        configs = this
            .pluginInterface.ConfigDirectory.GetFiles()
            .Select(x => x.Name.Substring(0, x.Name.Length - 5))
            .ToArray();
        if (selectedConfig >= configs.Length)
        {
            selectedConfig = 0;
        }
    }

    public void SaveNewDefaultConfig()
    {
        var count = 1;
        var newName = "new config";
        while (configs.Any(x => x == newName))
        {
            newName = $"new config {count}";
            count++;
        }

        var newConfig = new Config() { ConfigName = newName };

        SavePluginConfig(newConfig, newConfig.ConfigName);
    }

    public void DeleteConfig(string configName)
    {
        pluginLog.Debug($"Deleting config {configName}");
        var path = this.pluginInterface.ConfigDirectory.FullName + "/" + configName + ".json";
        var configFile = new FileInfo(path);
        if (configFile.Exists)
        {
            configFile.Delete();
        }

        UpdateConfigs();
    }

    private Config? Load(string configName)
    {
        var path = this.pluginInterface.ConfigDirectory.FullName + "/" + configName + ".json";
        FileInfo configFile = new FileInfo(path);
        pluginLog.Debug(configFile.FullName);
        return !configFile.Exists ? null : DeserializeConfig(File.ReadAllText(configFile.FullName));
    }

    internal void SavePluginConfig(Config? currentConfig, string configName)
    {
        if (currentConfig == null)
            return;
        var path = this.pluginInterface.ConfigDirectory.FullName + "/" + configName + ".json";
        this.Save(currentConfig, path);
        UpdateConfigs();
    }

    internal void Save(Config config, string path) =>
        this.WriteAllTextSafe(path, this.SerializeConfig(config));

    internal string SerializeConfig(Config config) =>
        JsonConvert.SerializeObject(
            config,
            Formatting.Indented,
            new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
            }
        );

    internal void WriteAllTextSafe(string path, string text)
    {
        var str = path + ".tmp";
        if (File.Exists(str))
            File.Delete(str);
        File.WriteAllText(str, text);
        File.Move(str, path, true);
    }

    internal static Config? DeserializeConfig(string data) =>
        JsonConvert.DeserializeObject<Config>(
            data,
            new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.None,
            }
        );
}
