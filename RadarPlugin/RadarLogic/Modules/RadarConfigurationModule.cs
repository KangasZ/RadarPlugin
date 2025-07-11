using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using RadarPlugin.Constants;
using RadarPlugin.Enums;

namespace RadarPlugin.RadarLogic.Modules;

public class RadarConfigurationModule : IModuleInterface
{
    private readonly Configuration.Configuration configInterface;
    private readonly IPlayerCharacter localPlayer;
    private readonly ZoneTypeModule zoneTypeModule;
    private readonly IClientState clientState;
    private readonly RankModule rankModule;
    private readonly DistanceModule distanceModule;
    private readonly MobLastMovement mobLastMovement;
    private readonly IPluginLog pluginLog;

    public RadarConfigurationModule(
        IClientState clientState,
        Configuration.Configuration configInterface,
        ZoneTypeModule zoneTypeModule,
        RankModule rankModule,
        DistanceModule distanceModule,
        MobLastMovement mobLastMovement,
        IPluginLog pluginLog
    )
    {
        this.configInterface = configInterface;
        this.zoneTypeModule = zoneTypeModule;
        this.clientState = clientState;
        this.rankModule = rankModule;
        this.distanceModule = distanceModule;
        this.mobLastMovement = mobLastMovement;
        this.pluginLog = pluginLog;
    }

    /**
 * TODO: Refactor this to be done once per second instead of on each render.
 */
    public string GetText(
        IGameObject gameObject,
        Configuration.Configuration.ESPOption espOption,
        bool radar3d = true
    )
    {
        var tagText = "";
        var displayTypeFlags =
            radar3d ? espOption.DisplayTypeFlags
            : espOption.Separate2DOptions ? espOption.DisplayTypeFlags2D
            : espOption.DisplayTypeFlags;

        if (displayTypeFlags.HasFlag(DisplayTypeFlags.Name))
        {
            if (
                MobConstants.DeepDungeonMapIds.Contains(this.clientState.TerritoryType)
                && MobConstants.RenameList.TryGetValue(gameObject.DataId, out var rename)
            )
            {
                tagText = rename;
            }
            else if (
                MobConstants.DeepDungeonMapIds.Contains(this.clientState.TerritoryType)
                && MobConstants.DeepDungeonMobTypesMap.TryGetValue(gameObject.DataId, out var value)
            )
            {
                tagText = value switch
                {
                    DeepDungeonMobTypes.GoldChest => "Gold Chest",
                    DeepDungeonMobTypes.SilverChest => "Silver Chest",
                    DeepDungeonMobTypes.BronzeChest => "Bronze Chest",
                    DeepDungeonMobTypes.AccursedHoard => "Accursed Hoard",
                    _ => gameObject.Name.TextValue,
                };
            }
            else if (string.IsNullOrWhiteSpace(gameObject.Name.TextValue))
            {
                tagText = "''";
            }
            else
            {
                tagText = gameObject.Name.TextValue;
            }
        }

        // Replace player names with job abbreviations
        if (
            espOption.ReplaceWithJobName
            && gameObject is IPlayerCharacter { ClassJob.Value: { } } pc
        )
        {
            tagText = pc.ClassJob.Value.Abbreviation.ToString();
        }

        // Append LEVEL and RANK to name
        if (gameObject is IBattleNpc battleNpc)
        {
            if (espOption.AppendLevelToName)
            {
                tagText += $" LV:{battleNpc.Level}";
            }

            // DEBUG rank text
            if (configInterface.cfg.RankText)
            {
                tagText += $"\nD {battleNpc.DataId}";
                tagText += $"\nN {battleNpc.NameId}";
                if (rankModule.TryGetRank(battleNpc.DataId, out byte value))
                {
                    tagText += $"\nR {value}";
                }
            }
        }

        // Draw distance
        if (displayTypeFlags.HasFlag(DisplayTypeFlags.Distance))
        {
            if (clientState.LocalPlayer != null)
                tagText += distanceModule
                    .GetDistanceFromPlayer(clientState.LocalPlayer, gameObject)
                    .ToString(" 0.0m");
        }

        // Draw Position
        if (displayTypeFlags.HasFlag(DisplayTypeFlags.Position))
        {
            tagText += $"({gameObject.Position.X:0.0}, {gameObject.Position.Z:0.0})";
        }

        if (
            configInterface.cfg.EXPERIMENTALEnableMobTimerTracking
            && gameObject.ObjectKind == ObjectKind.BattleNpc
            && (((IBattleNpc)gameObject).StatusFlags & StatusFlags.InCombat) == 0
        )
        {
            tagText += (
                mobLastMovement.GetTimeElapsedFromMovement(gameObject).TotalSeconds
            ).ToString(" 0.0s");
        }

        return configInterface.cfg.DebugText
            ? $"{gameObject.Name}, {gameObject.DataId}, {gameObject.ObjectKind}"
            : $"{tagText}";
    }

    public Configuration.Configuration.ESPOption TryGetOverridenParams(
        IGameObject areaObject,
        ulong obfuscatedSelfId,
        uint baseAccountId,
        out bool overridden
    )
    {
        overridden = false;
        Configuration.Configuration.ESPOptionMobBased? optionOverride = null;
        if (areaObject.ObjectKind == ObjectKind.Player)
        {
            var accountId = areaObject.GetDeobfuscatedAccountId(obfuscatedSelfId, baseAccountId);
            if (configInterface.cfg.PlayerOptionOverride.TryGetValue(accountId, out optionOverride))
            {
                overridden = true;
            }
        }
        else
        {
            if (
                configInterface.cfg.OptionOverride.TryGetValue(
                    areaObject.DataId,
                    out optionOverride
                )
            )
            {
                overridden = true;
            }
        }
        if (overridden)
        {
            // If the mob hasnt been updated in 100 seconds, update the name and time last seen
            if ((DateTime.UtcNow - optionOverride.UtcLastSeenTime).TotalSeconds > 100)
            {
                optionOverride.UtcLastSeenTime = DateTime.UtcNow;
                optionOverride.LastSeenName = areaObject.Name?.TextValue ?? "Unknown";
            }

            overridden = true;
            return optionOverride;
        }

        overridden = false;
        return GetParams(areaObject);
    }

    public Configuration.Configuration.ESPOption GetParams(IGameObject areaObject)
    {
        // If Deep Dungeon
        var zoneType = zoneTypeModule.GetLocationType();
        if (configInterface.cfg.ShowBaDdObjects && zoneType == LocationKind.DeepDungeon)
        {
            if (MobConstants.DeepDungeonMobTypesMap.TryGetValue(areaObject.DataId, out var value))
            {
                switch (value)
                {
                    case DeepDungeonMobTypes.SpecialUndead:
                        return configInterface.cfg.DeepDungeonOptions.SpecialUndeadOption;
                    case DeepDungeonMobTypes.Auspice:
                        return configInterface.cfg.DeepDungeonOptions.AuspiceOption;
                    case DeepDungeonMobTypes.EasyMobs:
                        return configInterface.cfg.DeepDungeonOptions.EasyMobOption;
                    case DeepDungeonMobTypes.Traps:
                        return configInterface.cfg.DeepDungeonOptions.TrapOption;
                    case DeepDungeonMobTypes.Return:
                        return configInterface.cfg.DeepDungeonOptions.ReturnOption;
                    case DeepDungeonMobTypes.Passage:
                        return configInterface.cfg.DeepDungeonOptions.PassageOption;
                    case DeepDungeonMobTypes.GoldChest:
                        return configInterface.cfg.DeepDungeonOptions.GoldChestOption;
                    case DeepDungeonMobTypes.SilverChest:
                        return configInterface.cfg.DeepDungeonOptions.SilverChestOption;
                    case DeepDungeonMobTypes.BronzeChest:
                        return configInterface.cfg.DeepDungeonOptions.BronzeChestOption;
                    case DeepDungeonMobTypes.AccursedHoard:
                        return configInterface.cfg.DeepDungeonOptions.AccursedHoardOption;
                    case DeepDungeonMobTypes.Mimic:
                        return configInterface.cfg.DeepDungeonOptions.MimicOption;
                    case DeepDungeonMobTypes.Default:
                        return configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption;
                    default:
                        return configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption;
                }
            }

            if (
                areaObject.ObjectKind == ObjectKind.BattleNpc
                && areaObject is IBattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy } mob
            )
            {
                return configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption;
            }
        }

        switch (areaObject.ObjectKind)
        {
            case ObjectKind.Player:
                if (areaObject is IPlayerCharacter chara)
                {
                    // Is the object is YOU
                    if (
                        configInterface.cfg.SeparatedYourPlayer.Enabled
                        && clientState.LocalPlayer != null
                        && chara.Address == clientState.LocalPlayer.Address
                    )
                    {
                        return configInterface.cfg.SeparatedYourPlayer.EspOption;
                    }

                    // If is friend
                    if (
                        configInterface.cfg.SeparatedFriends.Enabled
                        && chara.StatusFlags.HasFlag(StatusFlags.Friend)
                    ) //0x80
                    {
                        return configInterface.cfg.SeparatedFriends.EspOption;
                    }

                    // Is in party
                    if (
                        configInterface.cfg.SeparatedParty.Enabled
                        && chara.StatusFlags.HasFlag(StatusFlags.PartyMember)
                    ) //0x20
                    {
                        return configInterface.cfg.SeparatedParty.EspOption;
                    }

                    // If in alliance
                    if (
                        configInterface.cfg.SeparatedAlliance.Enabled
                        && chara.StatusFlags.HasFlag(StatusFlags.AllianceMember)
                    ) // 0x40
                    {
                        return configInterface.cfg.SeparatedAlliance.EspOption;
                    }
                }

                return configInterface.cfg.PlayerOption;
            case ObjectKind.Companion:
                return configInterface.cfg.CompanionOption;
            case ObjectKind.BattleNpc:
                if (areaObject is not IBattleChara bnpc)
                {
                    return configInterface.cfg.NpcOption;
                }

                if (
                    configInterface.cfg.SeparatedRankOne.Enabled
                    || configInterface.cfg.SeparatedRankTwoAndSix.Enabled
                )
                {
                    if (rankModule.TryGetRank(bnpc.DataId, out var value))
                    {
                        switch (value)
                        {
                            case 1:
                                if (configInterface.cfg.SeparatedRankOne.Enabled)
                                {
                                    return configInterface.cfg.SeparatedRankOne.EspOption;
                                }

                                break;
                            case 2:
                            case 6:
                                if (configInterface.cfg.SeparatedRankTwoAndSix.Enabled)
                                {
                                    return configInterface.cfg.SeparatedRankTwoAndSix.EspOption;
                                }

                                break;
                        }
                    }
                }

                if (
                    bnpc is IBattleNpc
                    {
                        BattleNpcKind: BattleNpcSubKind.Pet or BattleNpcSubKind.Chocobo
                    }
                )
                {
                    return configInterface.cfg.CompanionOption;
                }

                if (configInterface.cfg.LevelRendering.LevelRenderingEnabled)
                {
                    if (
                        clientState.LocalPlayer!.Level
                            - (byte)configInterface.cfg.LevelRendering.RelativeLevelsBelow
                        > bnpc.Level
                    )
                    {
                        return configInterface.cfg.LevelRendering.LevelRenderEspOption;
                    }
                }

                return configInterface.cfg.NpcOption;
            case ObjectKind.EventNpc:
                return configInterface.cfg.EventNpcOption;
            case ObjectKind.Treasure:
                return configInterface.cfg.TreasureOption;
            case ObjectKind.Aetheryte:
                return configInterface.cfg.AetheryteOption;
            case ObjectKind.GatheringPoint:
                return configInterface.cfg.GatheringPointOption;
            case ObjectKind.EventObj:
                return configInterface.cfg.EventObjOption;
            case ObjectKind.MountType:
                return configInterface.cfg.MountOption;
            case ObjectKind.Retainer:
                return configInterface.cfg.RetainerOption;
            case ObjectKind.Area:
                return configInterface.cfg.AreaOption;
            case ObjectKind.Housing:
                return configInterface.cfg.HousingOption;
            case ObjectKind.Cutscene:
                return configInterface.cfg.CutsceneOption;
            case ObjectKind.CardStand:
                return configInterface.cfg.CardStandOption;
            case ObjectKind.Ornament:
                return configInterface.cfg.OrnamentOption;
            case ObjectKind.None:
            default:
                return configInterface.cfg.NpcOption;
        }
    }

    public void Dispose()
    {
        //Do nothing
    }

    public void StartTick()
    {
        //Do Nothing
    }

    public void EndTick()
    {
        //do nothing
    }
}
