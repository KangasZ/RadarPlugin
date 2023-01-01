using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;
using RadarPlugin.Enums;
using RadarPlugin.UI;
using GameObject = Dalamud.Game.ClientState.Objects.Types.GameObject;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace RadarPlugin;

public class RadarLogic : IDisposable
{
    private const float PI = 3.14159265359f;
    private readonly DalamudPluginInterface pluginInterface;
    private Configuration configInterface;
    private readonly Condition conditionInterface;
    private Task backgroundLoop;
    private bool keepRunning;
    private readonly ObjectTable objectTable;
    private List<(GameObject, uint, string)> areaObjects; // Game object, color, string
    private readonly ClientState clientState;
    private readonly GameGui gameGui;
    private readonly RadarHelpers radarHelpers;


    public RadarLogic(DalamudPluginInterface pluginInterface, Configuration configuration, ObjectTable objectTable,
        Condition condition, ClientState clientState, GameGui gameGui, RadarHelpers radarHelpers)
    {
        // Creates Dependencies
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;
        this.conditionInterface = condition;
        this.gameGui = gameGui;
        this.radarHelpers = radarHelpers;

        // Loads plugin
        PluginLog.Debug("Radar Loaded");
        keepRunning = true;
        // TODO: In the future adjust this
        areaObjects = new List<(GameObject, uint, string)>();

        this.clientState = clientState;
        this.pluginInterface.UiBuilder.Draw += OnTick;
        backgroundLoop = Task.Run(BackgroundLoop);

        this.clientState.TerritoryChanged += CleanupZoneTerritoryWrapper;
        this.clientState.Logout += CleanupZoneLogWrapper;
        this.clientState.Login += CleanupZoneLogWrapper;
    }

    private void OnTick()
    {
        if (!configInterface.cfg.Enabled) return;
        if (objectTable.Length == 0) return;
        if (CheckDraw()) return;
        DrawRadar();
    }

    private void DrawRadar()
    {
        if (!Monitor.TryEnter(areaObjects))
        {
            PluginLog.Error("Try Enter Failed. This is not an error");
            return;
        }

        var drawListPtr = ImGui.GetForegroundDrawList();
        foreach (var areaObject in areaObjects)
        {
            var displayType = radarHelpers.GetDisplayType(areaObject.Item1);
            var dotSize = radarHelpers.GetDotSize(areaObject.Item1);
            var drawAggroCircleBools = radarHelpers.GetAggroCircleBools(areaObject.Item1);
            var drawDistance = radarHelpers.GetDistance(areaObject.Item1);
            DrawEsp(drawListPtr, areaObject.Item1, areaObject.Item2, areaObject.Item3, displayType, dotSize,
                drawAggroCircleBools.Item1, drawAggroCircleBools.Item2, drawDistance);
        }

        Monitor.Exit(areaObjects);
    }

    /**
     * Returns true if you should not draww
     */
    private bool CheckDraw()
    {
        return conditionInterface[ConditionFlag.LoggingOut] || conditionInterface[ConditionFlag.BetweenAreas] ||
               conditionInterface[ConditionFlag.BetweenAreas51] || !configInterface.cfg.Enabled ||
               clientState.LocalContentId == 0 || clientState.LocalPlayer == null;
    }

    private void DrawEsp(ImDrawListPtr drawListPtr, GameObject gameObject, uint color, string name,
        DisplayTypes displayTypes, float dotSize, bool drawAggroCircle, bool drawAggroCircleInCombat, bool drawDistance)
    {
        var visibleOnScreen = gameGui.WorldToScreen(gameObject.Position, out var onScreenPosition);
        if (visibleOnScreen)
        {
            switch (displayTypes)
            {
                case DisplayTypes.DotOnly:
                    DrawDot(drawListPtr, onScreenPosition, dotSize, color);
                    break;
                case DisplayTypes.NameOnly:
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, drawDistance);
                    break;
                case DisplayTypes.DotAndName:
                    DrawDot(drawListPtr, onScreenPosition, dotSize, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, drawDistance);
                    break;
                case DisplayTypes.HealthBarOnly:
                    DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthBarAndValue:
                    DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthBarAndName:
                    DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, drawDistance);
                    break;
                case DisplayTypes.HealthBarAndValueAndName:
                    DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, drawDistance);
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthValueOnly:
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthValueAndName:
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, drawDistance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            UiHelpers.GetBorderClampedVector2(onScreenPosition, new Vector2(10, 10), out var clampedPos);
            var mainViewport3 = ImGui.GetMainViewport();
            Vector2 center2 = (mainViewport3 ).GetCenter();
            Vector2 rotation = clampedPos - center2;
            float thickness = 2f;
            drawListPtr.DrawArrow(clampedPos, 5f, color, rotation, thickness);
        }

        if (drawAggroCircle && gameObject is BattleNpc npc2)
        {
            if (!drawAggroCircleInCombat && (npc2.StatusFlags & StatusFlags.InCombat) != 0) return;
            if (UtilInfo.AggroDistance.TryGetValue(gameObject.DataId, out var range))
            {
                DrawAggroRadius(drawListPtr, gameObject.Position, range + gameObject.HitboxRadius,
                    gameObject.Rotation,
                    uint.MaxValue);
            }
            else
            {
                DrawAggroRadius(drawListPtr, gameObject.Position, 10 + gameObject.HitboxRadius,
                    gameObject.Rotation,
                    uint.MaxValue);
            }
        }
    }

    private void DrawDot(ImDrawListPtr imDrawListPtr, Vector2 position, float radius, uint npcOptColor)
    {
        imDrawListPtr.AddCircleFilled(position, radius, npcOptColor, 100);
    }

    private void DrawHealthValue(ImDrawListPtr imDrawListPtr, Vector2 position, GameObject gameObject,
        uint playerOptColor)
    {
        if (gameObject is not BattleNpc npc) return;
        var healthText = ((int)(((double)npc.CurrentHp / npc.MaxHp) * 100)).ToString();
        var healthTextSize = ImGui.CalcTextSize(healthText);
        imDrawListPtr.AddText(
            new Vector2((position.X - healthTextSize.X / 2.0f), (position.Y - healthTextSize.Y / 2.0f)),
            playerOptColor,
            healthText);
    }

    private void DrawName(ImDrawListPtr imDrawListPtr, Vector2 position, string tagText, uint objectOptionColor,
        GameObject gameObject, bool drawDistance)
    {
        if (drawDistance)
        {
            tagText += " ";
            if (clientState.LocalPlayer != null)
                tagText += gameObject.Position.Distance2D(clientState.LocalPlayer.Position).ToString("0.0");
            tagText += "m";
        }

        var tagTextSize = ImGui.CalcTextSize(tagText);
        imDrawListPtr.AddText(
            new Vector2(position.X - tagTextSize.X / 2f, position.Y + tagTextSize.Y / 2f),
            objectOptionColor,
            tagText);
    }


    private void DrawHealthCircle(ImDrawListPtr imDrawListPtr, Vector2 position, GameObject gameObject,
        uint playerOptColor)
    {
        const float radius = 13f;
        if (gameObject is not BattleNpc npc) return;

        var v1 = (float)npc.CurrentHp / (float)npc.MaxHp;
        var aMax = PI * 2.0f;
        var difference = v1 - 1.0f;
        imDrawListPtr.PathArcTo(position, radius,
            (-(aMax / 4.0f)) + (aMax / npc.MaxHp) * (npc.MaxHp - npc.CurrentHp), aMax - (aMax / 4.0f), 200 - 1);
        imDrawListPtr.PathStroke(playerOptColor, ImDrawFlags.None, 2.0f);
    }

    private void DrawAggroRadius(ImDrawListPtr imDrawListPtr, Vector3 position, float radius, float rotation,
        uint objectOptionColor)
    {
        var opacity = configInterface.cfg.AggroRadiusOptions.CircleOpacity;
        rotation += MathF.PI / 4;
        var numSegments = 200;
        var segmentAngle = 2 * MathF.PI / numSegments;
        var points = new Vector2[numSegments];
        var onScreens = new bool[numSegments];
        var seg = 2 * MathF.PI / numSegments;
        var rot = rotation + 0 * MathF.PI;

        var originPointOnScreen = gameGui.WorldToScreen(
            new(position.X + radius * MathF.Sin(rot),
                position.Y,
                position.Z + radius * MathF.Cos(rot)),
            out var originPoint);

        for (int i = 0; i < numSegments; i++)
        {
            var a = rot - i * segmentAngle;
            var onScreen = gameGui.WorldToScreen(
                new(position.X + radius * MathF.Sin(a),
                    position.Y,
                    position.Z + radius * MathF.Cos(a)),
                out var p);
            points[i] = p;
            onScreens[i] = onScreen;
            if (onScreen)
            {
                imDrawListPtr.PathLineTo(p);
            }

            switch (i)
            {
                case 50:
                    imDrawListPtr
                        .PathStroke(configInterface.cfg.AggroRadiusOptions.FrontColor & opacity,
                            ImDrawFlags.RoundCornersAll, 4f);
                    // this forloop should only happen when cone shows (always right now)
                    for (int j = 0; j <= 50; j++)
                    {
                        imDrawListPtr.PathLineTo(points[j]);
                    }

                    var centeOnScreen = gameGui.WorldToScreen(
                        position,
                        out var centerPosition);
                    if (centeOnScreen)
                    {
                        imDrawListPtr.PathLineTo(centerPosition);
                    }
                    else
                    {
                        imDrawListPtr.PathClear();
                    }

                    imDrawListPtr.PathFillConvex(configInterface.cfg.AggroRadiusOptions.FrontConeColor &
                                                 configInterface.cfg.AggroRadiusOptions
                                                     .FrontConeOpacity);
                    imDrawListPtr.PathLineTo(p);
                    break;
                case 100:
                    imDrawListPtr
                        .PathStroke(configInterface.cfg.AggroRadiusOptions.RightSideColor & opacity,
                            ImDrawFlags.RoundCornersAll, 2f);
                    imDrawListPtr.PathLineTo(p);
                    break;
                case 150:
                    imDrawListPtr.PathStroke(configInterface.cfg.AggroRadiusOptions.RearColor & opacity,
                        ImDrawFlags.RoundCornersAll, 2f);
                    imDrawListPtr.PathLineTo(p);
                    break;
                case 199:
                    if (originPointOnScreen)
                    {
                        imDrawListPtr.PathLineTo(originPoint);
                    }

                    imDrawListPtr
                        .PathStroke(configInterface.cfg.AggroRadiusOptions.LeftSideColor & opacity,
                            ImDrawFlags.RoundCornersAll, 2f);
                    break;
            }
        }

        imDrawListPtr.PathClear();
    }

    private void BackgroundLoop()
    {
        while (keepRunning)
        {
            if (configInterface.cfg.Enabled)
            {
                if (CheckDraw())
                {
#if DEBUG
                    PluginLog.Verbose("Did not update mob info due to check fail.");
#endif
                }
                else
                {
                    var time = DateTime.Now;
                    UpdateMobInfo();
#if DEBUG
                    PluginLog.Verbose($"Refreshed Mob Info in {(DateTime.Now - time).TotalMilliseconds} ms.");
#endif
                }
            }

            Thread.Sleep(1000);
        }
    }

    private unsafe void UpdateMobInfo()
    {
        var nearbyMobs = new List<(GameObject, uint, string)>();
        foreach (var obj in objectTable)
        {
            if (!obj.IsValid()) continue;
            if (configInterface.cfg.DebugMode)
            {
                nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                continue;
            }

            if (clientState.LocalPlayer != null && obj.Address == clientState.LocalPlayer.Address)
            {
                if (configInterface.cfg.ShowYOU)
                {
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                }

                continue;
            }

            if (configInterface.cfg.ShowBaDdObjects)
            {
                // TODO: Check if we need to swap this out with a seperte eureka and potd list
                if (UtilInfo.RenameList.ContainsKey(obj.DataId) ||
                    UtilInfo.DeepDungeonMobTypesMap.ContainsKey(obj.DataId))
                {
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    continue;
                }
            }

            if (this.configInterface.cfg.ShowOnlyVisible &&
                ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)obj.Address)->RenderFlags != 0)
            {
                continue;
            }

            if (String.IsNullOrWhiteSpace(obj.Name.TextValue) && !configInterface.cfg.ShowNameless) continue;

            switch (obj.ObjectKind)
            {
                case ObjectKind.Treasure:
                    if (!configInterface.cfg.ShowLoot) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.Companion:
                    if (!configInterface.cfg.ShowCompanion) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.Area:
                    if (!configInterface.cfg.ShowAreaObjects) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.Aetheryte:
                    if (!configInterface.cfg.ShowAetherytes) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.EventNpc:
                    if (!configInterface.cfg.ShowEventNpc) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.EventObj:
                    if (!configInterface.cfg.ShowEvents) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.None:
                    break;
                case ObjectKind.Player:
                    if (!configInterface.cfg.ShowPlayers) continue;
                    //if (obj is not PlayerCharacter chara) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
                    break;
                case ObjectKind.BattleNpc:
                    if (obj is not BattleNpc mob) continue;
                    if (!configInterface.cfg.ShowEnemies) continue;
                    //if (String.IsNullOrWhiteSpace(mob.Name.TextValue)) continue;
                    if (mob.BattleNpcKind != BattleNpcSubKind.Enemy) continue;
                    if (mob.IsDead) continue;
                    if (UtilInfo.DataIdIgnoreList.Contains(mob.DataId) ||
                        configInterface.cfg.DataIdIgnoreList.Contains(mob.DataId)) continue;
                    nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
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
        pluginInterface.UiBuilder.Draw -= OnTick;
        clientState.TerritoryChanged -= CleanupZoneTerritoryWrapper;
        clientState.Logout -= CleanupZoneLogWrapper;
        clientState.Login -= CleanupZoneLogWrapper;
        keepRunning = false;
        while (!backgroundLoop.IsCompleted) ;
        Monitor.Enter(areaObjects);
        Monitor.Exit(areaObjects);
        PluginLog.Information("Radar Unloaded");
    }
}