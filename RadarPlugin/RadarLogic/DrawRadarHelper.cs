using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.UI;

namespace RadarPlugin.RadarLogic;

public static class DrawRadarHelper
{
    public static void DrawDot(
        ImDrawListPtr imDrawListPtr,
        Vector2 onScreenPosition,
        float radius,
        uint color
    )
    {
        //TODO: Num Segments should be configurable
        imDrawListPtr.AddCircleFilled(onScreenPosition, radius, color, 100);
    }

    public static void DrawTextCenteredUnder(
        ImDrawListPtr imDrawListPtr,
        Vector2 onScreenPosition,
        string textToDraw,
        uint color
    )
    {
        var tagTextSize = ImGui.CalcTextSize(textToDraw);
        imDrawListPtr.AddText(
            new Vector2(
                onScreenPosition.X - tagTextSize.X / 2f,
                onScreenPosition.Y + tagTextSize.Y / 2f
            ),
            color,
            textToDraw
        );
    }

    public static void DrawHealthCircle(
        ImDrawListPtr imDrawListPtr,
        Vector2 position,
        IGameObject gameObject,
        uint playerOptColor
    )
    {
        const float radius = 13f;
        if (gameObject is not IBattleChara npc)
            return;

        var v1 = (float)npc.CurrentHp / (float)npc.MaxHp;
        var aMax = MathF.PI * 2.0f;
        var difference = v1 - 1.0f;
        imDrawListPtr.PathArcTo(
            position,
            radius,
            (-(aMax / 4.0f)) + (aMax / npc.MaxHp) * (npc.MaxHp - npc.CurrentHp),
            aMax - (aMax / 4.0f),
            200 - 1
        );
        imDrawListPtr.PathStroke(playerOptColor, ImDrawFlags.None, 2.0f);
    }

    public static void DrawConeAtCenterPointFromRotation(
        ImDrawListPtr imDrawListPtr,
        Vector3 originPosition,
        float rotationStart,
        float totalRotationCw,
        float radius,
        uint color,
        int numSegments,
        IGameGui gameGui
    )
    {
        var rotationPerSegment = totalRotationCw / numSegments;
        var originOnScreen = gameGui.WorldToScreen(
            new Vector3(originPosition.X, originPosition.Y, originPosition.Z),
            out var originPositionOnScreen
        );
        if (!originOnScreen)
            return;
        imDrawListPtr.PathLineTo(originPositionOnScreen);
        for (var i = 0; i <= numSegments; i++)
        {
            var currentRotation = rotationStart - i * rotationPerSegment;
            var xValue = radius * MathF.Sin(currentRotation);
            var yValue = radius * MathF.Cos(currentRotation);
            var isOnScreen = gameGui.WorldToScreen(
                new Vector3(originPosition.X + xValue, originPosition.Y, originPosition.Z + yValue),
                out var segmentVectorOnCircle
            );
            //if (!isOnScreen) continue;
            imDrawListPtr.PathLineTo(segmentVectorOnCircle);
        }

        imDrawListPtr.PathFillConvex(color);
    }

    public static void DrawArcAtCenterPointFromRotations(
        ImDrawListPtr imDrawListPtr,
        Vector3 originPosition,
        float rotationStart,
        float totalRotationCw,
        float radius,
        uint color,
        float thickness,
        int numSegments,
        IGameGui gameGui
    )
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
                new Vector3(originPosition.X + xValue, originPosition.Y, originPosition.Z + yValue),
                out segmentVectorOnCircle
            );
            if (!isOnScreen)
            {
                imDrawListPtr.PathStroke(color, ImDrawFlags.RoundCornersAll, thickness);
                continue;
            }

            imDrawListPtr.PathLineTo(segmentVectorOnCircle);
        }

        imDrawListPtr.PathStroke(color, ImDrawFlags.RoundCornersAll, thickness);
    }

    public static void DrawHealthValue(
        ImDrawListPtr imDrawListPtr,
        Vector2 position,
        IGameObject gameObject,
        uint playerOptColor
    )
    {
        if (gameObject is not IBattleChara npc)
            return;
        var health = ((uint)(((double)npc.CurrentHp / npc.MaxHp) * 100));
        var healthText = health.ToString();
        var healthTextSize = ImGui.CalcTextSize(healthText);
        imDrawListPtr.AddText(
            new Vector2(
                (position.X - healthTextSize.X / 2.0f),
                (position.Y - healthTextSize.Y / 2.0f)
            ),
            playerOptColor,
            healthText
        );
    }

    public static void DrawHealthBar(
        ImDrawListPtr imDrawListPtr,
        Vector2 onScreenPositon,
        IGameObject gameObject
    )
    {
        UiHelpers.BufferingBar(
            imDrawListPtr,
            onScreenPositon,
            "hi",
            ConfigConstants.Black,
            ConfigConstants.Red,
            ConfigConstants.White,
            ConfigConstants.White,
            50f,
            100f,
            1f,
            0.5f
        );
    }
}
