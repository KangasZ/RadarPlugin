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
            PluginLog.Error("Try Enter Failed. This is not necessarily error");
            return;
        }

        var width = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;
        var height = ImGui.GetWindowContentRegionMax().Y - ImGui.GetWindowContentRegionMin().Y;
        var drawListPtr = ImGui.GetBackgroundDrawList();
        foreach (var areaObject in areaObjects)
        {
            var espOption = radarHelpers.GetParams(areaObject.Item1);
            DrawEsp(drawListPtr, areaObject.Item1, areaObject.Item2, areaObject.Item3, espOption);
        }

        Monitor.Exit(areaObjects);
    }

    /**
     * Returns true if you should not draww
     */
    private bool CheckDraw()
    {
        return conditionInterface[ConditionFlag.LoggingOut] || conditionInterface[ConditionFlag.BetweenAreas] ||
               conditionInterface[ConditionFlag.BetweenAreas51] ||
               clientState.LocalContentId == 0 || clientState.LocalPlayer == null;
    }

    private void DrawEsp(ImDrawListPtr drawListPtr, GameObject gameObject, uint color, string name,
        Configuration.ESPOption espOption)
    {
        var visibleOnScreen = gameGui.WorldToScreen(gameObject.Position, out var onScreenPosition);
        if (visibleOnScreen)
        {
            switch (espOption.DisplayType)
            {
                case DisplayTypes.DotOnly:
                    DrawDot(drawListPtr, onScreenPosition, espOption.DotSize, color);
                    break;
                case DisplayTypes.NameOnly:
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, espOption.DrawDistance);
                    break;
                case DisplayTypes.DotAndName:
                    DrawDot(drawListPtr, onScreenPosition, espOption.DotSize, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, espOption.DrawDistance);
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
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, espOption.DrawDistance);
                    break;
                case DisplayTypes.HealthBarAndValueAndName:
                    DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, espOption.DrawDistance);
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthValueOnly:
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthValueAndName:
                    DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    DrawName(drawListPtr, onScreenPosition, name, color, gameObject, espOption.DrawDistance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (configInterface.cfg.ShowOffScreen)
        {
            UiHelpers.GetBorderClampedVector2(onScreenPosition,
                new Vector2(configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge,
                    configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge), out var clampedPos);
            var mainViewport3 = ImGui.GetMainViewport();
            var center2 = mainViewport3.GetCenter();
            var rotation = clampedPos - center2;
            drawListPtr.DrawArrow(clampedPos, configInterface.cfg.OffScreenObjectsOptions.Size, color, rotation,
                configInterface.cfg.OffScreenObjectsOptions.Thickness);
        }

        if (gameObject is BattleNpc npc2)
        {
            if (espOption.ShowAggroCircle)
            {
                if (!espOption.ShowAggroCircleInCombat && (npc2.StatusFlags & StatusFlags.InCombat) != 0) return;
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

            if (configInterface.cfg.HitboxOptions.HitboxEnabled)
            {
                DrawHitbox(drawListPtr, gameObject.Position, gameObject.HitboxRadius,
                    configInterface.cfg.HitboxOptions.HitboxColor);
            }
        }
    }

    private void DrawHitbox(ImDrawListPtr drawListPtr, Vector3 gameObjectPosition, float gameObjectHitboxRadius,
        uint color)
    {
        var opacity = configInterface.cfg.AggroRadiusOptions.CircleOpacity;

        var thickness = 2f;

        //todo: handle CONE
        //todo: shove opacity into color 
        DrawArcAtCenterPointFromRotations(drawListPtr, gameObjectPosition, 0, 2 * MathF.PI, gameObjectHitboxRadius,
            color, thickness, 400);
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
        var numSegments = 100;

        var thickness = 2f;

        //todo: handle CONE
        //todo: shove opacity into color 
        var frontColor = configInterface.cfg.AggroRadiusOptions.FrontColor & opacity;
        DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation, MathF.PI / 2, radius, frontColor,
            thickness, numSegments);
        var rightColor = configInterface.cfg.AggroRadiusOptions.RightSideColor & opacity;
        DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation + MathF.PI / 2, MathF.PI / 2, radius,
            rightColor, thickness, numSegments);
        var backColor = configInterface.cfg.AggroRadiusOptions.RearColor & opacity;
        DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation + MathF.PI, MathF.PI / 2, radius,
            backColor, thickness, numSegments);
        var leftColor = configInterface.cfg.AggroRadiusOptions.LeftSideColor & opacity;
        DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation + (MathF.PI * 1.5f), MathF.PI / 2, radius,
            leftColor, thickness, numSegments);
        var coneColor = configInterface.cfg.AggroRadiusOptions.FrontConeColor &
                        configInterface.cfg.AggroRadiusOptions.FrontConeOpacity;
        DrawConeAtCenterPointFromRotation(imDrawListPtr, position, rotation, MathF.PI / 2, radius, coneColor, 50);

        imDrawListPtr.PathClear();
    }

    private void DrawArcAtCenterPointFromRotations(ImDrawListPtr imDrawListPtr, Vector3 originPosition,
        float rotationStart, float totalRotationCw, float radius, uint color, float thickness, int numSegments)
    {
        var rotationPerSegment = totalRotationCw / numSegments;
        Vector2 segmentVectorOnCircle;
        bool isOnScreen;
        for (var i = 0; i <= numSegments; i++)
        {
            var currentRotation = rotationStart - i * rotationPerSegment;
            var xValue = radius * MathF.Sin(currentRotation);
            var yValue = radius * MathF.Cos(currentRotation);
            isOnScreen = gameGui.WorldToScreen(
                new Vector3(originPosition.X + xValue,
                    originPosition.Y,
                    originPosition.Z + yValue),
                out segmentVectorOnCircle);
            if (!isOnScreen)
            {
                imDrawListPtr.PathStroke(color, ImDrawFlags.RoundCornersAll, thickness);
                continue;
            }

            imDrawListPtr.PathLineTo(segmentVectorOnCircle);
        }

        imDrawListPtr.PathStroke(color, ImDrawFlags.RoundCornersAll, thickness);
    }

    private void DrawConeAtCenterPointFromRotation(ImDrawListPtr imDrawListPtr, Vector3 originPosition,
        float rotationStart, float totalRotationCw, float radius, uint color, int numSegments)
    {
        var rotationPerSegment = totalRotationCw / numSegments;
        var originOnScreen = gameGui.WorldToScreen(
            new Vector3(originPosition.X,
                originPosition.Y,
                originPosition.Z),
            out var originPositionOnScreen);
        if (!originOnScreen) return;
        imDrawListPtr.PathLineTo(originPositionOnScreen);
        for (var i = 0; i <= numSegments; i++)
        {
            var currentRotation = rotationStart - i * rotationPerSegment;
            var xValue = radius * MathF.Sin(currentRotation);
            var yValue = radius * MathF.Cos(currentRotation);
            var isOnScreen = gameGui.WorldToScreen(
                new Vector3(originPosition.X + xValue,
                    originPosition.Y,
                    originPosition.Z + yValue),
                out var segmentVectorOnCircle);
            //if (!isOnScreen) continue;
            imDrawListPtr.PathLineTo(segmentVectorOnCircle);
        }

        imDrawListPtr.PathFillConvex(color);
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

    private void UpdateMobInfo()
    {
        var nearbyMobs = objectTable
            .Where(obj => obj.IsValid() && radarHelpers.ShouldRender(obj))
            .Select(obj => (obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj))).ToList();
        /*
         *foreach (var obj in objectTable)
         *{
         *  if (!obj.IsValid()) continue;
         *  if (radarHelpers.ShouldRender(obj))
         *  {
         *      nearbyMobs.Add((obj, radarHelpers.GetColor(obj), radarHelpers.GetText(obj)));
         *  }
         *}
         */

        Monitor.Enter(areaObjects);
        areaObjects.Clear();
        areaObjects.AddRange(nearbyMobs);
        Monitor.Exit(areaObjects);
    }


    private void CleanupZoneTerritoryWrapper(object? _, ushort __)
    {
        PluginLog.Debug($"New Territory: {clientState.TerritoryType}");
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