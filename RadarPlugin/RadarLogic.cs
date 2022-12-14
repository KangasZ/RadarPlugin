﻿using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using ImGuiNET;
using GameObject = Dalamud.Game.ClientState.Objects.Types.GameObject;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

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
    private ClientState clientState { get; set; }

    public RadarLogic(DalamudPluginInterface pluginInterface, Configuration configuration, ObjectTable objectTable,
        Condition condition, ClientState clientState)
    {
        // Creates Dependencies
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;
        this.conditionInterface = condition;

        // Loads plugin
        PluginLog.Debug("Radar Loaded");
        keepRunning = true;
        // TODO: In the future adjust this
        areaObjects = new List<GameObject>();

        this.clientState = clientState;
        this.pluginInterface.UiBuilder.Draw += DrawRadar;
        backgroundLoop = Task.Run(BackgroundLoop);

        this.clientState.TerritoryChanged += CleanupZoneTerritoryWrapper;
        this.clientState.Logout += CleanupZoneLogWrapper;
        this.clientState.Login += CleanupZoneLogWrapper;
    }

    private void DrawRadar()
    {
        if (!configInterface.cfg.Enabled) return;
        if (objectTable.Length == 0) return;
        if (CheckDraw()) return;

        if (!Monitor.TryEnter(areaObjects))
        {
            PluginLog.Error("Try Enter Failed. This is not an error");
            return;
        }

        foreach (var areaObject in areaObjects)
        {
            var p = Services.GameGui.WorldToScreen(areaObject.Position, out var onScreenPosition);
            if (!p) continue;

            var tagText = GetText(areaObject);
            uint color = UInt32.MinValue;

            DrawEsp(onScreenPosition, areaObject);
        }

        Monitor.Exit(areaObjects);
    }

    /**
     * Returns true if you should not draww
     */
    private bool CheckDraw()
    {
        return conditionInterface[ConditionFlag.LoggingOut] || conditionInterface[ConditionFlag.BetweenAreas] ||
               conditionInterface[ConditionFlag.BetweenAreas51] || !configInterface.cfg.Enabled;
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

                if (npcOpt.ShowDot)
                {
                    DrawDot(position, npcOpt.Color);
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

                if (playerOpt.ShowDot)
                {
                    DrawDot(position, playerOpt.Color);
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

                if (objectOption.ShowDot)
                {
                    DrawDot(position, objectOption.Color);
                }

                break;
        }
    }

    private void DrawDot(Vector2 position, Vector4 npcOptColor)
    {
        ImGui.GetForegroundDrawList().AddCircleFilled(position, 3f, ImGui.ColorConvertFloat4ToU32(npcOptColor), 100);
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
        var text = "";
        if (obj.DataId != 0 && UtilInfo.RenameList.ContainsKey(obj.DataId))
        {
            text = UtilInfo.RenameList[obj.DataId];
        }
        else if (String.IsNullOrWhiteSpace(obj.Name.TextValue))
        {
            text = "''";
        }
        else
        {
            text = obj.Name.TextValue;
        }

        return configInterface.cfg.DebugMode ? $"{obj.Name}, {obj.DataId}, {obj.ObjectKind}" : $"{text}";
    }

    private void BackgroundLoop()
    {
        while (keepRunning)
        {
            if (configInterface.cfg.Enabled)
            {
                if (CheckDraw())
                {
                    PluginLog.Verbose("Did not update mob info due to check fail.");
                }
                else
                {
                    var time = DateTime.Now;
                    UpdateMobInfo();
                    PluginLog.Verbose($"Refreshed Mob Info in {(DateTime.Now - time).TotalMilliseconds} ms.");
                }
            }

            Thread.Sleep(1000);
        }
    }

    private unsafe void UpdateMobInfo()
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

            if (this.configInterface.cfg.ShowOnlyVisible &&
                ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)obj.Address)->RenderFlags != 0)
            {
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
                    //if (obj is not PlayerCharacter chara) continue;
                    nearbyMobs.Add(obj);
                    break;
                case ObjectKind.BattleNpc:
                    if (obj is not BattleNpc mob) continue;
                    if (!configInterface.cfg.ShowEnemies) continue;
                    //if (String.IsNullOrWhiteSpace(mob.Name.TextValue)) continue;
                    if (mob.BattleNpcKind != BattleNpcSubKind.Enemy) continue;
                    if (mob.IsDead) continue;
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

        Monitor.Enter(areaObjects);
        areaObjects.Clear();
        areaObjects.AddRange(nearbyMobs);
        Monitor.Exit(areaObjects);
    }


    private void CleanupZoneTerritoryWrapper(object? _, ushort __)
    {
        CleanupZone();
    }

    private void CleanupZone()
    {
        PluginLog.Verbose("Clearing because of condition met.");
        Monitor.Enter(areaObjects);
        areaObjects.Clear();
        Monitor.Exit(areaObjects);
    }

    private void CleanupZoneLogWrapper(object? sender, EventArgs e)
    {
        CleanupZone();
    }

    public void Dispose()
    {
        this.pluginInterface.UiBuilder.Draw -= DrawRadar;
        clientState.TerritoryChanged -= CleanupZoneTerritoryWrapper;
        clientState.Logout -= CleanupZoneLogWrapper;
        clientState.Login -= CleanupZoneLogWrapper;
        keepRunning = false;
        while (!backgroundLoop.IsCompleted) ;
        Monitor.Enter(areaObjects);
        Monitor.Exit(areaObjects);
        PluginLog.Debug("Radar Unloaded");
    }
}