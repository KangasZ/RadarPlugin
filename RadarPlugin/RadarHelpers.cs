using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using RadarPlugin.Enums;

namespace RadarPlugin;

public class RadarHelpers
{
    private readonly Configuration configInterface;
    private readonly ClientState clientState;
    private readonly Condition conditionInterface;

    public RadarHelpers(
        Configuration configInterface,
        ClientState clientState,
        Condition condition
    )
    {
        this.clientState = clientState;
        this.configInterface = configInterface;
        this.conditionInterface = condition;
    }


    public unsafe bool ShouldRender(GameObject obj)
    {
        // Objest valid check
        if (!obj.IsValid()) return false;
        //if (obj.DataId == GameObject.InvalidGameObjectId) return false;
        
        // Object within ignore lists
        if (UtilInfo.DataIdIgnoreList.Contains(obj.DataId) || configInterface.cfg.DataIdIgnoreList.Contains(obj.DataId)) return false;
        
        // Object visible & config check
        var clientstructobj = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)obj.Address;
        if (configInterface.cfg.ShowOnlyVisible && (clientstructobj->RenderFlags != 0))
        {
            return false;
        }
        
        
        if (configInterface.cfg.ShowBaDdObjects && IsSpecialZone())
        {
            // UtilInfo.RenameList.ContainsKey(obj.DataId) || UtilInfo.DeepDungeonMobTypesMap.ContainsKey(obj.DataId)))
            if (UtilInfo.DeepDungeonMobTypesMap.ContainsKey(obj.DataId)) return true;
            if (string.IsNullOrWhiteSpace(obj.Name.TextValue) && !configInterface.cfg.ShowNameless) return false;
            if (obj.ObjectKind != ObjectKind.BattleNpc || obj is not BattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy } mob) return true;
            if (!configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption.Enabled) return false;
            return !mob.IsDead;
        }
        

        if (obj is BattleChara mobNpc)
        {
            //if (!clientstructobj->GetIsTargetable()) continue;
            //if (String.IsNullOrWhiteSpace(mob.Name.TextValue)) continue;
            if (string.IsNullOrWhiteSpace(obj.Name.TextValue) && !configInterface.cfg.ShowNameless) return false;
            if (mobNpc.IsDead) return false;
        }

        return true;
    }

    public bool IsSpecialZone()
    {
        return UtilInfo.DeepDungeonMapIds.Contains(this.clientState.TerritoryType) ||
               this.conditionInterface[ConditionFlag.InDeepDungeon];
    }

    /**
     * TODO: Refactor this to be done once per second instead of on each render.
     */
    public string GetText(GameObject obj)
    {
        var text = "";
        if (obj.DataId != 0 && UtilInfo.DeepDungeonMapIds.Contains(this.clientState.TerritoryType) &&
            UtilInfo.RenameList.ContainsKey(obj.DataId))
        {
            text = UtilInfo.RenameList[obj.DataId];
        }
        else if (string.IsNullOrWhiteSpace(obj.Name.TextValue))
        {
            text = "''";
        }
        else
        {
            text = obj.Name.TextValue;
        }

        return configInterface.cfg.DebugMode ? $"{obj.Name}, {obj.DataId}, {obj.ObjectKind}" : $"{text}";
    }

    public uint? GetColorOverride(GameObject gameObject)
    {
        // Override over all
        if (configInterface.cfg.ColorOverride.TryGetValue(gameObject.DataId, out var colorOverride))
        {
            return colorOverride;
        }
        return null;
    }

    public Configuration.ESPOption GetParams(GameObject areaObject)
    {
        // If Deep Dungeon
        if (configInterface.cfg.ShowBaDdObjects && IsSpecialZone())
        {
            if (UtilInfo.DeepDungeonMobTypesMap.TryGetValue(areaObject.DataId, out var value))
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

            if (areaObject.ObjectKind == ObjectKind.BattleNpc && areaObject is BattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy } mob)
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
                    if (configInterface.cfg.SeparateYourPlayer && clientState.LocalPlayer != null && chara.Address == clientState.LocalPlayer.Address)
                    {
                        return configInterface.cfg.YourPlayerOption;
                    }
                    
                    // If is friend
                    if (configInterface.cfg.SeparateFriends && chara.StatusFlags.HasFlag(StatusFlags.Friend)) //0x80
                    {
                        return configInterface.cfg.FriendOption;
                    }
                    
                    // Is in party
                    if (configInterface.cfg.SeparateParty && chara.StatusFlags.HasFlag(StatusFlags.PartyMember)) //0x20
                    {
                        return configInterface.cfg.PartyOption;
                    }
                    
                    // If in alliance
                    if (configInterface.cfg.SeparateAlliance && chara.StatusFlags.HasFlag(StatusFlags.AllianceMember)) // 0x40
                    {
                        return configInterface.cfg.AllianceOption;
                    }
                }
                return configInterface.cfg.PlayerOption;
            case ObjectKind.Companion:
                return configInterface.cfg.CompanionOption;
            case ObjectKind.BattleNpc:
                if (areaObject is BattleNpc { BattleNpcKind: BattleNpcSubKind.Pet or BattleNpcSubKind.Chocobo})
                {
                    return configInterface.cfg.CompanionOption;
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
}