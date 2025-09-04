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
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.Configuration;

[Serializable]
public class Configuration
{
    public class ConeSettings
    {
        public bool Enabled = false;
        public uint ConeColor = Color.Gray50;
        public bool Fill = true;
        public float Radius = ConfigConstants.DefaultConeRadius;
        public float RadianAngle = ConfigConstants.DefaultConeAngleRadians; // About sqrt(2)/2 or 45 degrees (from each side)
    }

    public class Radar2DConfiguration
    {
        public bool Enabled = false;
        public bool ShowBackground = true;
        public uint BackgroundColor = Color.BackgroundDefault;
        public bool Clickthrough = false;
        public bool ShowCross = true;
        public uint CrossColor = Color.White;
        public bool ShowRadarBorder = true;
        public bool ShowSettings = true;
        public bool ShowScale = true;
        public float Scale = 5f;
        public bool ShowYourCurrentPosition = true;
        public bool RotationLockedNorth = false;
        public ConeSettings PlayerConeSettings = new ConeSettings() { ConeColor = Color.Gray50 };
        public ConeSettings CameraConeSettings = new ConeSettings()
        {
            ConeColor = Color.LightBlue50,
        };
    }

    public class LevelRendering
    {
        public bool LevelRenderingEnabled = false;
        public int RelativeLevelsBelow = 20;
        public ESPOption LevelRenderEspOption = new(mobOptDefault);
    }

    public class FontSettings
    {
        public bool UseCustomFont = false;
        public bool UseAxisFont = false;
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
        public bool OverrideMobColor = false;
        public uint HitboxColor = Color.Turquoise;
        public float Thickness = 2.2f;

        public bool DrawInsideCircle = false;
        public uint InsideCircleOpacity = 0xffffffff;
        public bool UseDifferentInsideCircleColor = false;
        public uint InsideCircleColor = Color.Turquoise & 0x50ffffff;
    }

    public class OffScreenObjectsOptions
    {
        public float Thickness = 2.2f;
        public float DistanceFromEdge = 15f;
        public float Size = 6.0f;
    }

    public class DeepDungeonOptions
    {
        public ESPOption SpecialUndeadOption { get; set; } =
            new(mobOptDefault) { ColorU = Color.Yellow };
        public ESPOption AuspiceOption { get; set; } = new(mobOptDefault) { ColorU = Color.Green };
        public ESPOption EasyMobOption { get; set; } =
            new(mobOptDefault) { ColorU = Color.LightBlue };
        public ESPOption TrapOption { get; set; } = new(objectOptDefault) { ColorU = Color.Orange };
        public ESPOption ReturnOption { get; set; } = new(objectOptDefault) { ColorU = Color.Blue };
        public ESPOption PassageOption { get; set; } =
            new(objectOptDefault) { ColorU = Color.Blue };
        public ESPOption GoldChestOption { get; set; } =
            new(objectOptDefault) { ColorU = Color.Gold };
        public ESPOption SilverChestOption { get; set; } =
            new(objectOptDefault) { ColorU = Color.Silver };
        public ESPOption BronzeChestOption { get; set; } =
            new(objectOptDefault) { ColorU = Color.Bronze };
        public ESPOption MimicOption { get; set; } = new(mobOptDefault) { ColorU = Color.Red };

        public ESPOption AccursedHoardOption { get; set; } =
            new(objectOptDefault) { ColorU = ConfigConstants.Turquoise };

        public ESPOption DefaultEnemyOption { get; set; } =
            new(mobOptDefault) { ColorU = ConfigConstants.White };
    }

    public class AggroRadiusOptions
    {
        public bool ShowAggroCircle = false;
        public bool ShowAggroCircleInCombat = false;
        public bool MaxDistanceCapBool = true;
        public float MaxDistance = ConfigConstants.DefaultMaxAggroRadiusDistance;
        public uint FrontColor = ConfigConstants.Red;
        public uint RearColor = ConfigConstants.Green;
        public uint RightSideColor = ConfigConstants.Yellow;
        public uint LeftSideColor = ConfigConstants.Yellow;
        public uint FrontConeColor = ConfigConstants.Red;
        public uint CircleOpacity = 0xBEFFFFFF;
        public uint FrontConeOpacity = 0x30FFFFFF;
        public uint SoundAggroColor = ConfigConstants.Turquoise;
        public uint ProximityAggroColor = ConfigConstants.Red;
    }

    public class SeparatedEspOption
    {
        public bool Enabled = false;
        public ESPOption EspOption = new(objectOptDefault);
    }

    public class ESPOption
    {
        public ESPOption() { }

        // Copy Constructor
        public ESPOption(ESPOption espOption)
        {
            Enabled = espOption.Enabled;
            DisplayType = espOption.DisplayType;
            this.DisplayTypeFlags = espOption.DisplayTypeFlags;
            ColorU = espOption.ColorU;
            DrawDistance = espOption.DrawDistance;
            AppendLevelToName = espOption.AppendLevelToName;
        }

        public bool Enabled = true;

        /*
        public bool ShowDot = true;
        public bool ShowHp = false;
        public bool ReplaceDotWithHP = false;
        public bool ShowName = true;*/
        public bool ShowMp = false;

        [Obsolete]
        public DisplayTypes DisplayType = DisplayTypes.NameOnly;
        public uint ColorU = 0xffffffff;
        public bool DrawDistance = false;
        public bool DotSizeOverride = false;
        public float DotSize = ConfigConstants.DefaultDotSize;
        public bool ReplaceWithJobName = false;
        public bool AppendLevelToName = false;
        public DisplayTypeFlags DisplayTypeFlags = DisplayTypeFlags.Default;

        public bool Separate2DOptions = false;
        public DisplayTypeFlags DisplayTypeFlags2D = DisplayTypeFlags.Default;
        public bool DotSizeOverride2D = false;
        public float DotSize2D = ConfigConstants.DefaultDotSize;
    }

    public class ESPOptionMobBased : ESPOption
    {
        public ESPOptionMobBased() { }

        public ESPOptionMobBased(ESPOption espOption)
        {
            Enabled = espOption.Enabled;
            DisplayType = espOption.DisplayType;
            DisplayTypeFlags = espOption.DisplayTypeFlags;
            DisplayTypeFlags2D = espOption.DisplayTypeFlags2D;
            ColorU = espOption.ColorU;
            DrawDistance = espOption.DrawDistance;
            AppendLevelToName = espOption.AppendLevelToName;
        }

        public ESPOptionMobBased(
            ESPOption espOption,
            string name,
            ulong id,
            MobType mobType = MobType.Object,
            ulong tertiaryId = 0
        )
        {
            Name = name;
            Enabled = espOption.Enabled;
            DisplayType = espOption.DisplayType;
            DisplayTypeFlags = espOption.DisplayTypeFlags;
            DisplayTypeFlags2D = espOption.DisplayTypeFlags2D;
            ColorU = espOption.ColorU;
            DrawDistance = espOption.DrawDistance;
            AppendLevelToName = espOption.AppendLevelToName;
            MobTypeValue = mobType;
            this.Id = id;
            this.TertiaryId = tertiaryId;
        }

        public ulong TertiaryId = 0;
        public ulong Id = 0;
        public DateTime UtcLastSeenTime = DateTime.UtcNow;
        public string LastSeenName = string.Empty;
        public MobType MobTypeValue = MobType.Object;
        public string Name = string.Empty;
    }

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
        public ESPOption NpcOption { get; set; } =
            new(mobOptDefault) { Enabled = true, AppendLevelToName = false };
        public ESPOption PlayerOption { get; set; } = new(playerOptDefault);
        public ESPOption TreasureOption { get; set; } = new(objectOptDefault) { Enabled = true };
        public ESPOption CompanionOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption AreaOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption AetheryteOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption EventNpcOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption EventObjOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption GatheringPointOption { get; set; } =
            new(objectOptDefault) { Enabled = false };
        public ESPOption MountOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption RetainerOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption HousingOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption CutsceneOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption CardStandOption { get; set; } = new(objectOptDefault) { Enabled = false };
        public ESPOption OrnamentOption { get; set; } = new(objectOptDefault) { Enabled = false };
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
            EspOption = new ESPOption(playerOptDefault) { ColorU = ConfigConstants.Gold },
        };

        public SeparatedEspOption SeparatedYourPlayer = new()
        {
            EspOption = new ESPOption(playerOptDefault) { ColorU = ConfigConstants.Turquoise },
        };

        public SeparatedEspOption SeparatedParty = new()
        {
            EspOption = new ESPOption(playerOptDefault) { ColorU = ConfigConstants.Turquoise },
        };

        public SeparatedEspOption SeparatedFriends = new()
        {
            EspOption = new ESPOption(playerOptDefault) { ColorU = ConfigConstants.Orange },
        };

        public SeparatedEspOption SeparatedRankOne = new()
        {
            EspOption = new ESPOption(mobOptDefault) { ColorU = ConfigConstants.Gold },
        };

        public SeparatedEspOption SeparatedRankTwoAndSix = new()
        {
            EspOption = new ESPOption(mobOptDefault) { ColorU = ConfigConstants.Yellow },
        };

        public bool EXPERIMENTALEnableMobTimerTracking = false;
        public Radar2DConfiguration Radar2DConfiguration = new();
    }

    public Config cfg;

    [NonSerialized]
    private IDalamudPluginInterface pluginInterface;

    [NonSerialized]
    private static readonly ESPOption playerOptDefault = new ESPOption
    {
        Enabled = true,
        ColorU = 0xffff00ff,
        DisplayType = DisplayTypes.DotAndName,
        DisplayTypeFlags = DisplayTypes.DotAndName.ToFlags(),
        DisplayTypeFlags2D = DisplayTypes.DotAndName.ToFlags(),
        DrawDistance = false,
    };

    [NonSerialized]
    public static readonly ESPOption objectOptDefault = new ESPOption
    {
        Enabled = true,
        ColorU = 0xffFFFF00,
        DisplayType = DisplayTypes.NameOnly,
        DisplayTypeFlags = DisplayTypes.NameOnly.ToFlags(),
        DisplayTypeFlags2D = DisplayTypes.NameOnly.ToFlags(),
        DrawDistance = false,
    };

    [NonSerialized]
    private static readonly ESPOption mobOptDefault = new ESPOption
    {
        Enabled = true,
        ColorU = 0xffffffff,
        DisplayType = DisplayTypes.HealthValueAndName,
        DisplayTypeFlags = DisplayTypes.HealthValueAndName.ToFlags(),
        DisplayTypeFlags2D = DisplayTypes.HealthValueAndName.ToFlags(),
        DrawDistance = false,
    };

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
        if (gameObject.ObjectKind == ObjectKind.Player)
        {
            var accountIdT = gameObject.GetDeobfuscatedAccountId(obfuscatedSelfId, yourBaseId);
            var contentId = gameObject.GetContentId();
            if (accountIdT == 0)
            {
                return;
            }

            var accountId = accountIdT;
            if (customizeEnabled)
            {
                var newSettings = new ESPOptionMobBased(
                    currentSettings,
                    gameObject.Name.TextValue ?? "Oops it broke :(",
                    accountId,
                    MobType.Player,
                    tertiaryId: contentId
                );
                if (cfg.PlayerOptionOverride.ContainsKey(accountId))
                {
                    cfg.PlayerOptionOverride.Remove(accountId);
                }

                cfg.PlayerOptionOverride.Add(accountId, newSettings);
            }
            else
            {
                cfg.PlayerOptionOverride.Remove(accountId);
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
