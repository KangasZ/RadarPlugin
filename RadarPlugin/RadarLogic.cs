using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using RadarPlugin;

namespace RadarPlugin;

public class RadarLogic : IDisposable
{
    private const float PI = 3.14159265359f;
    private DalamudPluginInterface pluginInterface { get; set; }
    private Configuration configInterface { get; set; }
    private Condition conditionInterface { get; set; }
    private Task backgroundLoop { get; set; }
    private bool keepRunning { get; set; }
    private ObjectTable objectTable { get; set; }
    private List<GameObject> areaObjects { get; set; }
    private bool refreshing { get; set; }

    public RadarLogic(DalamudPluginInterface pluginInterface, Configuration configuration, ObjectTable objectTable, Condition condition)
    {
        // Creates Dependencies
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;
        this.conditionInterface = condition;

        // Loads plugin
        PluginLog.Debug($"Radar Loaded");
        keepRunning = true;
        // TODO: In the future adjust this
        this.pluginInterface.UiBuilder.Draw += DrawRadar;
        backgroundLoop = Task.Run(BackgroundLoop);

        areaObjects = new List<GameObject>();
    }

    private void DrawRadar()
    {
        if (!configInterface.cfg.Enabled) return;
        if (objectTable.Length == 0) return;
        if (this.conditionInterface[ConditionFlag.LoggingOut]) return;
        if (refreshing) return;
        foreach (var areaObject in areaObjects)
        {
            var p = Services.GameGui.WorldToScreen(areaObject.Position, out var onScreenPosition);
            if (!p) continue;

            var tagText = GetText(areaObject);
            uint color = UInt32.MinValue;

            DrawEsp(onScreenPosition, areaObject);
        }
    }

    private void DrawEsp(Vector2 position, GameObject gameObject)
    {
        switch (gameObject)
        {
            // Mobs
            case BattleNpc mob:
                var npcOpt = configInterface.cfg.NpcOption;
                if (npcOpt.ShowHealthBar)
                {
                    DrawHealthCircle(position, mob.MaxHp, mob.CurrentHp, npcOpt.Color);
                }
                if (npcOpt.ShowHealthValue)
                {
                    DrawHealthValue(position, mob.MaxHp, mob.CurrentHp, npcOpt.Color);
                }
                if (npcOpt.ShowName)
                {
                    var tagText = GetText(gameObject);
                    DrawName(position, tagText, npcOpt.Color);
                }

                break;
            // Players
            case PlayerCharacter chara:
                var playerOpt = configInterface.cfg.PlayerOption;
                //var hp = chara.CurrentHp / chara.MaxHp;
                if (playerOpt.ShowHealthBar)
                {
                    DrawHealthCircle(position, chara.MaxHp, chara.CurrentHp, playerOpt.Color);
                }
                
                if (playerOpt.ShowHealthValue)
                {
                    DrawHealthValue(position, chara.MaxHp, chara.CurrentHp, playerOpt.Color);
                }

                if (playerOpt.ShowName)
                {
                    var tagText = GetText(gameObject);
                    DrawName(position, tagText, playerOpt.Color);
                }

                break;
            // Event Objects
            case EventObj chara:
            // Npcs
            case Npc npc:
            // Objects
            default:
                var objectOption = configInterface.cfg.ObjectOption;
                if (objectOption.ShowName)
                {
                    var tagText = GetText(gameObject);
                    DrawName(position, tagText, objectOption.Color);
                }

                break;
        }
    }
    
    private void DrawHealthValue(Vector2 position, uint maxHp, uint currHp, Vector4 playerOptColor)
    {
        var healthText = ((int)(((double)currHp / maxHp) * 100)).ToString();
        var healthTextSize = ImGui.CalcTextSize(healthText);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2((position.X - healthTextSize.X / 2.0f), (position.Y - healthTextSize.Y / 2.0f)),
            ImGui.ColorConvertFloat4ToU32(playerOptColor),
            healthText);
        
    }

    private void DrawName(Vector2 position, string tagText, Vector4 objectOptionColor)
    {
        var tagTextSize = ImGui.CalcTextSize(tagText);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2(position.X - tagTextSize.X / 2f, position.Y + tagTextSize.Y / 2f),
            ImGui.ColorConvertFloat4ToU32(objectOptionColor),
            tagText);
    }


    private void DrawHealthCircle(Vector2 position, uint maxHp, uint currHp, Vector4 playerOptColor)
    {
        const float radius = 13f;
        
        var v1 = (float)currHp / (float)maxHp;
        var aMax = PI * 2.0f;
        var difference = v1 - 1.0f;
        var colorHealth = ImGui.ColorConvertFloat4ToU32(playerOptColor);
        ImGui.GetForegroundDrawList().PathArcTo(position, radius,
            (-(aMax / 4.0f)) + (aMax / maxHp) * (maxHp - currHp), aMax - (aMax / 4.0f), 200 - 1);
        ImGui.GetForegroundDrawList().PathStroke(colorHealth, ImDrawFlags.None, 2.0f);
    }

    private string GetText(GameObject obj)
    {
        if (obj.DataId != 0 && UtilInfo.RenameList.ContainsKey(obj.DataId))
        {
            return UtilInfo.RenameList[obj.DataId];
        }

        return configInterface.cfg.DebugMode ? $"{obj.Name}, {obj.DataId}, {obj.ObjectKind}" : $"{obj.Name}";
    }

    private void BackgroundLoop()
    {
        while (keepRunning)
        {
            if (configInterface.cfg.Enabled)
            {
                UpdateMobInfo();
                PluginLog.Debug("Refreshed Mob Info!");
            }

            Thread.Sleep(1000);
        }
    }

    private void UpdateMobInfo()
    {
        var nearbyMobs = new List<GameObject>();

        foreach (var obj in objectTable)
        {
            if (!obj.IsValid()) continue;
            if (configInterface.cfg.DebugMode)
            {
                nearbyMobs.Add(obj);
                continue;
            }

            switch (obj.ObjectKind)
            {
                case ObjectKind.Treasure:
                    if (!configInterface.cfg.ShowLoot) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.Companion:
                    if (!configInterface.cfg.ShowCompanion) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.Area:
                    if (!configInterface.cfg.ShowAreaObjects) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.Aetheryte:
                    if (!configInterface.cfg.ShowAetherytes) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.EventNpc:
                    if (!configInterface.cfg.ShowEventNpc) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.EventObj:
                    if (configInterface.cfg.ShowBaDdObjects)
                    {
                        if (UtilInfo.RenameList.ContainsKey(obj.DataId)) // Portal and some potd stuff
                        {
                            nearbyMobs.Add(obj);
                            continue;
                        }
                    }

                    if (!configInterface.cfg.ShowEvents) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.None:
                    break;
                case ObjectKind.Player:
                    if (!configInterface.cfg.ShowPlayers) continue;
                    if (obj is not PlayerCharacter chara) continue;
                    if (chara.CurrentHp <= 0) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.BattleNpc:
                    if (obj is not BattleNpc mob) continue;
                    if (!configInterface.cfg.ShowEnemies) continue;
                    if (String.IsNullOrWhiteSpace(mob.Name.TextValue)) continue;
                    if (mob.BattleNpcKind != BattleNpcSubKind.Enemy) continue;
                    if (mob.CurrentHp <= 0) continue;
                    if (UtilInfo.DataIdIgnoreList.Contains(mob.DataId) ||
                        configInterface.cfg.DataIdIgnoreList.Contains(mob.DataId)) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.GatheringPoint:
                    break;
                case ObjectKind.MountType:
                    break;
                case ObjectKind.Retainer:
                    break;
                case ObjectKind.Housing:
                    break;
                case ObjectKind.Cutscene:
                    break;
                case ObjectKind.CardStand:
                    break;
                default:
                    break;
            }
        }

        refreshing = true; // TODO change to mutex off refreshing
        areaObjects.Clear();
        areaObjects.AddRange(nearbyMobs);
        refreshing = false;
    }

    public void Dispose()
    {
        this.pluginInterface.UiBuilder.Draw -= DrawRadar;
        keepRunning = false;
        while (!backgroundLoop.IsCompleted) ;
        PluginLog.Debug($"Radar Unloaded");
    }
}