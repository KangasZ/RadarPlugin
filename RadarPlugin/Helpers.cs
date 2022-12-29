using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using RadarPlugin.Enums;

namespace RadarPlugin;

public class Helpers
{
    private readonly Configuration configInterface;
    private readonly ClientState clientState;

    public Helpers(
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
        if (obj.DataId != 0 && UtilInfo.RenameList.ContainsKey(obj.DataId))
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
        if (configInterface.cfg.ColorOverride.ContainsKey(gameObject.DataId))
        {
            return configInterface.cfg.ColorOverride[gameObject.DataId];
        }

        if (configInterface.cfg.ShowBaDdObjects && UtilInfo.DeepDungeonMapIds.Contains(this.clientState.TerritoryType))
        {
            if (UtilInfo.DeepDungeonMobTypesMap.ContainsKey(gameObject.DataId))
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
            }
            else
            {
                return configInterface.cfg.DeepDungeonMobTypeColorOptions.Default;
            }
        }

        switch (gameObject.ObjectKind)
        {
            case ObjectKind.Player:
                return configInterface.cfg.PlayerOption.ColorU;
            case ObjectKind.BattleNpc:
                return configInterface.cfg.NpcOption.ColorU;
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
                return configInterface.cfg.ObjectOption.ColorU;
        }
    }
}