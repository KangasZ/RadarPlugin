using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using RadarPlugin.Enums;

namespace RadarPlugin;

public class RadarHelpers
{
    private readonly Configuration configInterface;
    private readonly ClientState clientState;

    public RadarHelpers(
        Configuration configInterface,
        ClientState clientState
    )
    {
        this.clientState = clientState;
        this.configInterface = configInterface;
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

    public uint GetColor(GameObject gameObject)
    {
        // Override over all
        if (configInterface.cfg.ColorOverride.ContainsKey(gameObject.DataId))
        {
            return configInterface.cfg.ColorOverride[gameObject.DataId];
        }

        // If Deep Dungeon
        if (configInterface.cfg.ShowBaDdObjects && UtilInfo.DeepDungeonMobTypesMap.ContainsKey(gameObject.DataId) &&
            UtilInfo.DeepDungeonMapIds.Contains(this.clientState.TerritoryType) &&
            gameObject.ObjectKind != ObjectKind.Player)
        {
            switch (UtilInfo.DeepDungeonMobTypesMap[gameObject.DataId])
            {
                case DeepDungeonMobTypes.SpecialUndead:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.SpecialUndead;
                case DeepDungeonMobTypes.Auspice:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.Auspice;
                case DeepDungeonMobTypes.EasyMobs:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.EasyMobs;
                case DeepDungeonMobTypes.Traps:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.Traps;
                case DeepDungeonMobTypes.Return:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.Return;
                case DeepDungeonMobTypes.Passage:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.Passage;
                case DeepDungeonMobTypes.GoldChest:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.GoldChest;
                case DeepDungeonMobTypes.SilverChest:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.SilverChest;
                case DeepDungeonMobTypes.BronzeChest:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.BronzeChest;
                case DeepDungeonMobTypes.AccursedHoard:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.AccursedHoard;
                case DeepDungeonMobTypes.Mimic:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.Mimic;
                case DeepDungeonMobTypes.Default:
                default:
                    return configInterface.cfg.DeepDungeonMobTypeColorOptions.Default;
            }

            return configInterface.cfg.DeepDungeonMobTypeColorOptions.Default;
        }

        // Finally if nothing else
        switch (gameObject.ObjectKind)
        {
            case ObjectKind.Player:
                return configInterface.cfg.PlayerOption.ColorU;
            case ObjectKind.BattleNpc:
                return configInterface.cfg.NpcOption.ColorU;
            case ObjectKind.EventNpc:
                return configInterface.cfg.EventNpcOption.ColorU;
            case ObjectKind.Treasure:
                return configInterface.cfg.TreasureOption.ColorU;
            case ObjectKind.Aetheryte:
                return configInterface.cfg.AetheryteOption.ColorU;
            case ObjectKind.GatheringPoint:
                return configInterface.cfg.GatheringPointOption.ColorU;
            case ObjectKind.EventObj:
                return configInterface.cfg.EventObjOption.ColorU;
            case ObjectKind.MountType:
                return configInterface.cfg.MountOption.ColorU;
            case ObjectKind.Companion:
                return configInterface.cfg.CompanionOption.ColorU;
            case ObjectKind.Retainer:
                return configInterface.cfg.RetainerOption.ColorU;
            case ObjectKind.Area:
                return configInterface.cfg.AreaOption.ColorU;
            case ObjectKind.Housing:
                return configInterface.cfg.HousingOption.ColorU;
            case ObjectKind.Cutscene:
                return configInterface.cfg.CutsceneOption.ColorU;
            case ObjectKind.CardStand:
                return configInterface.cfg.CardStandOption.ColorU;
            case ObjectKind.None:
            default:
                return configInterface.cfg.ObjectOption.ColorU;
        }
    }

    public Configuration.ESPOption GetParams(GameObject areaObject)
    {
        switch (areaObject.ObjectKind)
        {
            case ObjectKind.Player:
                return configInterface.cfg.PlayerOption;
            case ObjectKind.BattleNpc:
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
            case ObjectKind.Companion:
                return configInterface.cfg.CompanionOption;
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
            case ObjectKind.None:
            default:
                return configInterface.cfg.ObjectOption;
        }
    }
}