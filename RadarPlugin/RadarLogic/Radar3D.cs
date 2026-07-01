using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.UI;

namespace RadarPlugin.RadarLogic;

public unsafe class Radar3D
{
    private readonly RadarModules radarModules;
    private readonly IPluginLog pluginLog;
    private readonly IClientState clientState;
    private readonly IGameGui gameGui;
    private readonly Configuration.Configuration configInterface;
    private readonly IObjectTable objectTable;

    public Radar3D(
        Configuration.Configuration configuration,
        IClientState clientState,
        IGameGui gameGui,
        IPluginLog pluginLog,
        RadarModules radarModules,
        IObjectTable objectTable
    )
    {
        // Creates Dependencies
        this.configInterface = configuration;
        this.gameGui = gameGui;
        this.radarModules = radarModules;
        this.pluginLog = pluginLog;
        // Loads plugin
        this.pluginLog.Debug("Radar Loaded");

        this.clientState = clientState;
        this.objectTable = objectTable;
    }

    public void Radar3DOnTick(
        IEnumerable<(
            IGameObject areaObject,
            Configuration.Configuration.ESPOption espOption
        )> objectTableRef
    )
    {
        // Setup Drawlist
        if (!configInterface.cfg.Radar3DEnabled)
            return;
        var bgDl = configInterface.cfg.UseBackgroundDrawList;
        var requiresSave = false;
        ImDrawListPtr drawListPtr;
        if (bgDl)
        {
            drawListPtr = ImGui.GetBackgroundDrawList();
        }
        else
        {
            ImGui.Begin(
                "RadarPlugin3DOverlay",
                ImGuiWindowFlags.NoMove
                    | ImGuiWindowFlags.NoInputs
                    | ImGuiWindowFlags.NoScrollbar
                    | ImGuiWindowFlags.NoMouseInputs
                    | ImGuiWindowFlags.NoScrollWithMouse
                    | ImGuiWindowFlags.NoBackground
                    | ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoBringToFrontOnFocus
                    | ImGuiWindowFlags.NoResize
                    | ImGuiWindowFlags.NoNav
                    | ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.NoDocking
                    | ImGuiWindowFlags.NoFocusOnAppearing
            );
            var mainViewPort = ImGui.GetMainViewport();

            ImGui.SetWindowPos(mainViewPort.Pos);
            ImGui.SetWindowSize(mainViewPort.Size);
            drawListPtr = ImGui.GetWindowDrawList();
        }

        foreach (var (areaObject, espOption) in objectTableRef)
        {
            if (!espOption.Enabled && !configInterface.cfg.DebugMode)
                continue;
            DrawEsp(drawListPtr, areaObject, espOption.ColorU, espOption);
        }

        if (!bgDl)
        {
            ImGui.End();
        }
    }

    private void DrawEsp(
        ImDrawListPtr drawListPtr,
        IGameObject gameObject,
        uint? overrideColor,
        Configuration.Configuration.ESPOption espOption
    )
    {
        var color = overrideColor ?? espOption.ColorU;
        var visibleOnScreen = gameGui.WorldToScreen(gameObject.Position, out var onScreenPosition);
        if (visibleOnScreen)
        {
            var dotSize = espOption.DotSizeOverride
                ? espOption.DotSize
                : configInterface.cfg.DotSize;

            // Assuming espOption.DisplayTypeFlags is the variable of type DisplayTypeFlags
            var displayFlags = espOption.DisplayTypeFlags;

            if (displayFlags.HasFlag(DisplayTypeFlags.Dot))
            {
                DrawRadarHelper.DrawDot(drawListPtr, onScreenPosition, dotSize, color);
            }

            if (
                displayFlags.HasFlag(DisplayTypeFlags.Name)
                || displayFlags.HasFlag(DisplayTypeFlags.Distance)
                || displayFlags.HasFlag(DisplayTypeFlags.Position)
            )
            {
                var nameText = radarModules.radarConfigurationModule.GetText(gameObject, espOption);
                DrawRadarHelper.DrawTextCenteredUnder(
                    drawListPtr,
                    onScreenPosition,
                    nameText,
                    color
                );
            }

            if (displayFlags.HasFlag(DisplayTypeFlags.HealthCircle))
            {
                DrawRadarHelper.DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
            }

            if (displayFlags.HasFlag(DisplayTypeFlags.HealthValue))
            {
                DrawRadarHelper.DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
            }

            if (displayFlags.HasFlag(DisplayTypeFlags.HealthBar))
            {
                //DrawRadarHelper.DrawHealthBar(drawListPtr, onScreenPosition, gameObject);
            }
        }
        else if (configInterface.cfg.ShowOffScreen)
        {
            UiHelpers.GetBorderClampedVector2(
                onScreenPosition,
                new Vector2(
                    configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge,
                    configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge
                ),
                out var clampedPos
            );
            var mainViewport3 = ImGui.GetMainViewport();
            var center2 = mainViewport3.GetCenter();
            var rotation = clampedPos - center2;
            drawListPtr.DrawArrow(
                clampedPos,
                configInterface.cfg.OffScreenObjectsOptions.Size,
                color,
                rotation,
                configInterface.cfg.OffScreenObjectsOptions.Thickness
            );
        }

        switch (gameObject)
        {
            case IBattleNpc npc2:
            {
                if (configInterface.cfg.HitboxOptions.HitboxEnabled)
                {
                    uint colorResolved;

                    if (configInterface.cfg.HitboxOptions.OverrideMobColor)
                    {
                        colorResolved = configInterface.cfg.HitboxOptions.HitboxColor;
                    }
                    else
                    {
                        colorResolved = color;
                    }

                    //opacity = configInterface.cfg.AggroRadiusOptions.CircleOpacity;
                    DrawHitbox(
                        drawListPtr,
                        gameObject.Position,
                        gameObject.HitboxRadius,
                        colorResolved,
                        configInterface.cfg.HitboxOptions.Thickness
                    );

                    if (configInterface.cfg.HitboxOptions.DrawInsideCircle)
                    {
                        uint insideColorResolved;
                        if (configInterface.cfg.HitboxOptions.UseDifferentInsideCircleColor)
                        {
                            insideColorResolved = configInterface
                                .cfg
                                .HitboxOptions
                                .InsideCircleColor;
                        }
                        else
                        {
                            insideColorResolved =
                                colorResolved
                                & configInterface.cfg.HitboxOptions.InsideCircleOpacity;
                        }

                        DrawRadarHelper.DrawConeAtCenterPointFromRotation(
                            drawListPtr,
                            gameObject.Position,
                            gameObject.Rotation,
                            MathF.PI * 2,
                            gameObject.HitboxRadius,
                            insideColorResolved,
                            100,
                            gameGui
                        );
                    }
                }

                if (configInterface.cfg.AggroRadiusOptions.ShowAggroCircle)
                {
                    // Aggro radius max distance check
                    if (
                        configInterface.cfg.AggroRadiusOptions.MaxDistanceCapBool
                        && objectTable.LocalPlayer != null
                        && radarModules.distanceModule.GetDistanceFromPlayer(
                            objectTable.LocalPlayer,
                            npc2
                        ) > configInterface.cfg.AggroRadiusOptions.MaxDistance
                    )
                    {
                        return;
                    }

                    if (
                        !configInterface.cfg.AggroRadiusOptions.ShowAggroCircleInCombat
                        && (npc2.StatusFlags & StatusFlags.InCombat) != 0
                    )
                        return;
                    if (npc2.BattleNpcKind != BattleNpcSubKind.Combatant)
                        return;
                    float aggroRadius = 10;
                    if (MobConstants.AggroDistance.TryGetValue(gameObject.DataId, out var range))
                    {
                        aggroRadius = range;
                    }

                    Vector3? playerPosition = configInterface.cfg.AggroRadiusOptions.EnableMaxDistanceArcFromPlayer
                        ? objectTable.LocalPlayer.Position
                        : null;
                    var gameObjectPosition = new Vector3(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
                    if (configInterface.cfg.AggroRadiusOptions.ShowAggroCircleOnPlayerHeight)
                    {
                        gameObjectPosition.Y = objectTable.LocalPlayer.Position.Y;
                    }
                    DrawAggroRadius(
                        drawListPtr,
                        gameObjectPosition,
                        aggroRadius + gameObject.HitboxRadius,
                        gameObject.Rotation,
                        npc2,
                        playerPosition,
                        configInterface.cfg.AggroRadiusOptions.MaxDistanceArcFromPlayer
                    );
                }

                break;
            }
            case IPlayerCharacter pc when espOption.ShowMp:
                DrawMp(drawListPtr, onScreenPosition, pc, color);
                break;
        }
    }

    //todo better
    private void DrawMp(
        ImDrawListPtr imDrawListPtr,
        Vector2 position,
        IPlayerCharacter gameObject,
        uint playerOptColor
    )
    {
        var mpText = gameObject.CurrentMp.ToString();
        var mptextSize = ImGui.CalcTextSize(mpText);
        imDrawListPtr.AddText(
            new Vector2((position.X - mptextSize.X / 2.0f), (position.Y + mptextSize.Y * 1.5f)),
            playerOptColor,
            mpText
        );
    }

    private void DrawHitbox(
        ImDrawListPtr drawListPtr,
        Vector3 gameObjectPosition,
        float gameObjectHitboxRadius,
        uint color,
        float thickness
    )
    {
        DrawRadarHelper.DrawArcAtCenterPointFromRotations(
            drawListPtr,
            gameObjectPosition,
            0,
            2 * MathF.PI,
            gameObjectHitboxRadius,
            color,
            thickness,
            400,
            gameGui
        );
    }

    private void DrawAggroRadius(
        ImDrawListPtr imDrawListPtr,
        Vector3 position,
        float radius,
        float rotation,
        IBattleNpc battleNpc,
        Vector3? playerPosition = null,
        float maxDistanceFromPlayer = 0f
    )
    {
        rotation += MathF.PI / 4;
        var numSegments = 100;

        var thickness = 2f;

        //todo: handle CONE
        //todo: shove opacity into color
        var aggroType = radarModules.aggroTypeModule.GetAggroType(battleNpc.NameId);
        switch (aggroType)
        {
            case AggroType.Proximity:
                var proximityColor = configInterface.cfg.AggroRadiusOptions.FrontColor;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(
                    imDrawListPtr,
                    position,
                    rotation,
                    MathF.PI * 2,
                    radius,
                    proximityColor,
                    thickness,
                    numSegments,
                    gameGui,
                    playerPosition,
                    maxDistanceFromPlayer
                );
                break;
            case AggroType.Sound:
                var soundColor = configInterface.cfg.AggroRadiusOptions.RearColor;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(
                    imDrawListPtr,
                    position,
                    rotation,
                    MathF.PI * 2,
                    radius,
                    soundColor,
                    thickness,
                    numSegments,
                    gameGui,
                    playerPosition,
                    maxDistanceFromPlayer
                );
                break;
            case AggroType.Sight:
            default:
                var frontColor = configInterface.cfg.AggroRadiusOptions.FrontColor;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(
                    imDrawListPtr,
                    position,
                    rotation,
                    MathF.PI / 2,
                    radius,
                    frontColor,
                    thickness,
                    numSegments,
                    gameGui,
                    playerPosition,
                    maxDistanceFromPlayer
                );
                var rightColor = configInterface.cfg.AggroRadiusOptions.RightSideColor;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(
                    imDrawListPtr,
                    position,
                    rotation + MathF.PI / 2,
                    MathF.PI / 2,
                    radius,
                    rightColor,
                    thickness,
                    numSegments,
                    gameGui,
                    playerPosition,
                    maxDistanceFromPlayer
                );
                var backColor = configInterface.cfg.AggroRadiusOptions.RearColor;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(
                    imDrawListPtr,
                    position,
                    rotation + MathF.PI,
                    MathF.PI / 2,
                    radius,
                    backColor,
                    thickness,
                    numSegments,
                    gameGui,
                    playerPosition,
                    maxDistanceFromPlayer
                );
                var leftColor = configInterface.cfg.AggroRadiusOptions.LeftSideColor;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(
                    imDrawListPtr,
                    position,
                    rotation + (MathF.PI * 1.5f),
                    MathF.PI / 2,
                    radius,
                    leftColor,
                    thickness,
                    numSegments,
                    gameGui,
                    playerPosition,
                    maxDistanceFromPlayer
                );
                var coneColor = configInterface.cfg.AggroRadiusOptions.FrontConeColor;
                DrawRadarHelper.DrawConeAtCenterPointFromRotation(
                    imDrawListPtr,
                    position,
                    rotation,
                    MathF.PI / 2,
                    radius,
                    coneColor,
                    50,
                    gameGui
                );
                break;
        }

        imDrawListPtr.PathClear();
    }
}
