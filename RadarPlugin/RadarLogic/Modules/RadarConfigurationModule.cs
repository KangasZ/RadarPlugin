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
    private readonly PlayerCharacter localPlayer;
    private readonly ZoneTypeModule zoneTypeModule;
    private readonly IClientState clientState;
    private readonly RankModule rankModule;
    private readonly DistanceModule distanceModule;
    private readonly MobLastMovement mobLastMovement;

    public RadarConfigurationModule(IClientState clientState, Configuration.Configuration configInterface,
        ZoneTypeModule zoneTypeModule, RankModule rankModule, DistanceModule distanceModule,
        MobLastMovement mobLastMovement)
    {
        this.configInterface = configInterface;
        this.zoneTypeModule = zoneTypeModule;
        this.clientState = clientState;
        this.rankModule = rankModule;
        this.distanceModule = distanceModule;
        this.mobLastMovement = mobLastMovement;
    }

    /**
 * TODO: Refactor this to be done once per second instead of on each render.
 */
    public string GetText(GameObject gameObject, Configuration.Configuration.ESPOption espOption)
    {
        var tagText = "";
        if (gameObject.DataId != 0 && MobConstants.DeepDungeonMapIds.Contains(this.clientState.TerritoryType) &&
            MobConstants.RenameList.ContainsKey(gameObject.DataId))
        {
            tagText = MobConstants.RenameList[gameObject.DataId];
        }
        else if (string.IsNullOrWhiteSpace(gameObject.Name.TextValue))
        {
            tagText = "''";
        }
        else
        {
            tagText = gameObject.Name.TextValue;
        }

        // Replace player names with job abbreviations
        if (espOption.ReplaceWithJobName && gameObject is PlayerCharacter { ClassJob.GameData: { } } pc)
        {
            tagText = pc.ClassJob.GameData.Abbreviation.RawString;
        }

        // Append LEVEL and RANK to name
        if (gameObject is BattleNpc battleNpc)
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
        if (espOption.DrawDistance)
        {
            if (clientState.LocalPlayer != null)
                tagText += distanceModule.GetDistanceFromPlayer(clientState.LocalPlayer, gameObject).ToString(" 0.0m");
        }

        if (configInterface.cfg.EXPERIMENTALEnableMobTimerTracking
            && gameObject.ObjectKind == ObjectKind.BattleNpc
            && (((BattleNpc)gameObject).StatusFlags & StatusFlags.InCombat) == 0)
        {
            tagText += (mobLastMovement.GetTimeElapsedFromMovement(gameObject).TotalSeconds)
                .ToString(" 0.0s");
        }

        return configInterface.cfg.DebugText
            ? $"{gameObject.Name}, {gameObject.DataId}, {gameObject.ObjectKind}"
            : $"{tagText}";
    }

    public Configuration.Configuration.ESPOption GetParamsWithOverride(GameObject areaObject)
    {
        // If overridden
        if (configInterface.cfg.OptionOverride.TryGetValue(areaObject.DataId, out var optionOverride))
        {
            if ((DateTime.UtcNow - optionOverride.UtcLastSeenTime).TotalSeconds > 100)
            {
                optionOverride.UtcLastSeenTime = DateTime.UtcNow;
                optionOverride.LastSeenName = areaObject.Name?.TextValue ?? "Unknown";
            }

            return optionOverride;
        }

        return GetParams(areaObject);
    }

    public Configuration.Configuration.ESPOption GetParams(GameObject areaObject)
    {
        // If Deep Dungeon
        var zoneType = zoneTypeModule.GetLocationType();
        if (configInterface.cfg.ShowBaDdObjects && zoneType == LocationKind.DeepDungeon)
        {
            if (MobConstants.DeepDungeonMobTypesMap.TryGetValue(areaObject.DataId, out var value))
            {
                switch (value)
                {
                    case DeepDungeonMobTypes.Default:
                        return configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption;
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
                    default:
                        return configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption;
                }
            }

            if (areaObject.ObjectKind == ObjectKind.BattleNpc && areaObject is BattleNpc
                {
                    BattleNpcKind: BattleNpcSubKind.Enemy
                } mob)
            {
                return configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption;
            }
        }

        switch (areaObject.ObjectKind)
        {
            case ObjectKind.Player:
                if (areaObject is PlayerCharacter chara)
                {
                    // Is the object is YOU
                    if (configInterface.cfg.SeparatedYourPlayer.Enabled && clientState.LocalPlayer != null &&
                        chara.Address == clientState.LocalPlayer.Address)
                    {
                        return configInterface.cfg.SeparatedYourPlayer.EspOption;
                    }

                    // If is friend
                    if (configInterface.cfg.SeparatedFriends.Enabled &&
                        chara.StatusFlags.HasFlag(StatusFlags.Friend)) //0x80
                    {
                        return configInterface.cfg.SeparatedFriends.EspOption;
                    }

                    // Is in party
                    if (configInterface.cfg.SeparatedParty.Enabled &&
                        chara.StatusFlags.HasFlag(StatusFlags.PartyMember)) //0x20
                    {
                        return configInterface.cfg.SeparatedParty.EspOption;
                    }

                    // If in alliance
                    if (configInterface.cfg.SeparatedAlliance.Enabled &&
                        chara.StatusFlags.HasFlag(StatusFlags.AllianceMember)) // 0x40
                    {
                        return configInterface.cfg.SeparatedAlliance.EspOption;
                    }
                }

                return configInterface.cfg.PlayerOption;
            case ObjectKind.Companion:
                return configInterface.cfg.CompanionOption;
            case ObjectKind.BattleNpc:
                if (areaObject is not BattleChara bnpc)
                {
                    return configInterface.cfg.NpcOption;
                }

                if (configInterface.cfg.SeparatedRankOne.Enabled || configInterface.cfg.SeparatedRankTwoAndSix.Enabled)
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

                if (bnpc is BattleNpc { BattleNpcKind: BattleNpcSubKind.Pet or BattleNpcSubKind.Chocobo })
                {
                    return configInterface.cfg.CompanionOption;
                }

                if (configInterface.cfg.LevelRendering.LevelRenderingEnabled)
                {
                    if (clientState.LocalPlayer!.Level - (byte)configInterface.cfg.LevelRendering.RelativeLevelsBelow >
                        bnpc.Level)
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