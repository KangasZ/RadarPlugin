using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

namespace RadarPlugin.UI;

public unsafe class Radar2D
{
    private readonly Configuration.Configuration configuration;
    private bool configWindowVisible = false;
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly IClientState clientState;
    private readonly IPluginLog pluginLog;
    private readonly RadarModules radarModules;

    public Radar2D(IDalamudPluginInterface dalamudPluginInterface, Configuration.Configuration configuration,
        IClientState clientState, IPluginLog pluginLog, RadarModules radarModules)
    {
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.configuration = configuration;
        this.clientState = clientState;
        this.pluginLog = pluginLog;
        this.radarModules = radarModules;
    }

    public void Radar2DOnTick(
        IEnumerable<(IGameObject areaObject, Configuration.Configuration.ESPOption espOption)> gameObjects)
    {
        var config = configuration.cfg.Radar2DConfiguration;
        if (!config.Enabled) return;
        var flags = config.Clickthrough
            ? ImGuiWindowFlags.NoMove |
              ImGuiWindowFlags.NoInputs |
              ImGuiWindowFlags.NoScrollbar |
              ImGuiWindowFlags.NoMouseInputs |
              ImGuiWindowFlags.NoScrollWithMouse |
              ImGuiWindowFlags.NoTitleBar |
              ImGuiWindowFlags.NoResize |
              ImGuiWindowFlags.NoNav |
              ImGuiWindowFlags.NoDecoration |
              ImGuiWindowFlags.NoDocking |
              ImGuiWindowFlags.NoFocusOnAppearing
            : ImGuiWindowFlags.NoTitleBar;
        if (!config.ShowBackground)
        {
            flags |= ImGuiWindowFlags.NoBackground;
        }

        ImGui.SetNextWindowSize(new Vector2(
                300,
                300),
            ImGuiCond.FirstUseEver);


        ImGui.PushStyleColor(ImGuiCol.WindowBg, config.BackgroundColor);
        ImGui.Begin("RadarPlugin2DRadar", flags);
        
        DrawRadar(gameObjects);

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    private void DrawRadar(
        IEnumerable<(IGameObject areaObject, Configuration.Configuration.ESPOption espOption)> gameObjects)
    {
        var imDrawListPtr = ImGui.GetWindowDrawList();
        var region = ImGui.GetContentRegionAvail();
        var start = ImGui.GetWindowPos();
        if (configuration.cfg.Radar2DConfiguration.ShowSettings)
        {
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Cog.ToIconString()}##-settings"))
            {
                ImGui.OpenPopup("Radar2DPopupSettings");
            }

            ImGui.PopFont();
        }
        
        if (clientState.LocalPlayer == null) return;
        var playerRotation = clientState.LocalPlayer.Rotation;
        var playerPosition = clientState.LocalPlayer.Position;
        if (configuration.cfg.Radar2DConfiguration.ShowYourCurrentPosition)
        {
            var positionText = $"({playerPosition.X:0.0}, {playerPosition.Z:0.0})";
            var textSize = ImGui.CalcTextSize(positionText);
            ImGui.SameLine(ImGui.GetWindowWidth()-textSize.X-10);
            ImGui.Text(positionText);
        } 
        
        Draw2DRadarPopupSettings();

        if (configuration.cfg.Radar2DConfiguration.ShowScale)
        {
            ImGui.SliderFloat("Scale", ref configuration.cfg.Radar2DConfiguration.Scale, 0.3f, 30f);
        }

        var xBegin = start.X + 10;
        var yBegin = start.Y + 10;
        var xEnd = xBegin + region.X - 5;
        var yEnd = yBegin + region.Y;

        var regionSize = new Vector2(region.X - 5, region.Y);
        var regionOffset = new Vector2(xBegin, yBegin);

        /* Debug stuff
        imDrawListPtr.AddCircle(regionOffset, 2, ConfigConstants.Red, 60, 5);
        imDrawListPtr.AddCircle(regionOffset + regionSize, 2, ConfigConstants.Turquoise, 60, 5);
        imDrawListPtr.AddCircle(regionOffset + regionSize with { X = 0 }, 2, ConfigConstants.Blue, 60, 5);
        imDrawListPtr.AddCircle(regionOffset + regionSize with {Y = 0}, 2, ConfigConstants.Gold, 60, 5);
        */
        var center = regionSize with { X = regionSize.X / 2, Y = regionSize.Y / 2 } + regionOffset;
        //imDrawListPtr.AddCircle(center, 2, ConfigConstants.Silver, 60, 5);
        if (configuration.cfg.Radar2DConfiguration.ShowCross)
        {
            imDrawListPtr.AddLine(new Vector2(center.X, regionOffset.Y),
                new Vector2(center.X, regionOffset.Y + regionSize.Y), configuration.cfg.Radar2DConfiguration.CrossColor, 1);
            imDrawListPtr.AddLine(new Vector2(regionOffset.X, center.Y),
                new Vector2(regionOffset.X + regionSize.X, center.Y), configuration.cfg.Radar2DConfiguration.CrossColor, 1);
        }

        var cameraManager = CameraManager.Instance();
        var currCamera = cameraManager->CurrentCamera;
        //pluginLog.Debug($"{currCamera->LookAtVector.X}, {currCamera->LookAtVector.Y}, {currCamera->LookAtVector.Z}");

        Vector3 camFront = new Vector3(currCamera->ViewMatrix.M13, currCamera->ViewMatrix.M23,
            currCamera->ViewMatrix.M33);
        float pitch = (float)Math.Asin(camFront.Y);

        var cameraRotation = (float)Math.Atan2(-camFront.Z / Math.Cos(pitch), -camFront.X / Math.Cos(pitch));
        var cameraRotationOffset = configuration.cfg.Radar2DConfiguration.RotationLockedNorth ? 0 : cameraRotation + Single.Pi / 2;
        foreach (var (areaObject, espOption) in gameObjects)
        {
            if (!espOption.Enabled && !configuration.cfg.DebugMode) continue;
            var color = espOption.ColorU;

            var difference = areaObject.Position - playerPosition;
            var diff2 = new Vector2(difference.X, difference.Z);

            // Scale the difference by the setter
            diff2 *= configuration.cfg.Radar2DConfiguration.Scale;

            // Rotate the difference vector by the negative of the camera's rotation
            Vector2 rotatedDiff;
            if (configuration.cfg.Radar2DConfiguration.RotationLockedNorth) {
                rotatedDiff = diff2;
            } else
            {
                rotatedDiff = diff2.Rotate(cameraRotationOffset);
                //var cos = Math.Cos(-cameraRotation);
                //var sin = Math.Sin(-cameraRotation);
                //rotatedDiff = new Vector2((float)(diff2.X * cos - diff2.Y * sin),
                //    (float)(diff2.X * sin + diff2.Y * cos));
            }
            
            // Dot position
            var position = center + rotatedDiff;
            var displayTypeFlags = espOption.Separate2DOptions ? espOption.DisplayTypeFlags2D : espOption.DisplayTypeFlags;
            if (displayTypeFlags.HasFlag(DisplayTypeFlags.Dot))
            {
                var dotSize = espOption.DotSizeOverride2D ? espOption.DotSize2D : configuration.cfg.DotSize2D;
                DrawRadarHelper.DrawDot(imDrawListPtr, position, dotSize, color);
            }

            if (displayTypeFlags.HasFlag(DisplayTypeFlags.Name) || displayTypeFlags.HasFlag(DisplayTypeFlags.Distance) || displayTypeFlags.HasFlag(DisplayTypeFlags.Position)) 
            {
                var nameText = radarModules.radarConfigurationModule.GetText(areaObject, espOption, false);
                DrawRadarHelper.DrawTextCenteredUnder(imDrawListPtr, position, nameText, color);
            }

            if (displayTypeFlags.HasFlag(DisplayTypeFlags.HealthCircle))
            {
                DrawRadarHelper.DrawHealthCircle(imDrawListPtr, position, areaObject, color);
            }

            if (displayTypeFlags.HasFlag(DisplayTypeFlags.HealthValue))
            {
                DrawRadarHelper.DrawHealthValue(imDrawListPtr, position, areaObject, color);
            }
        }
        
        // Camera Cone
        var cameraConeSettings = configuration.cfg.Radar2DConfiguration.CameraConeSettings;
        if (cameraConeSettings.Enabled)
        {
            imDrawListPtr.PathLineTo(center);
            //var diffRight = new Vector2(50, -50);
            //diffRight = diffRight.Rotate(cameraRotation);
            //imDrawListPtr.PathLineTo(center + diffLeft);
            var angleOffset = cameraConeSettings.RadianAngle;
            var cameraDirection = -(cameraRotationOffset - cameraRotation);
            imDrawListPtr.PathArcTo(center, cameraConeSettings.Radius, cameraDirection - angleOffset, cameraDirection + angleOffset, 200);
            imDrawListPtr.PathLineTo(center);
            if (cameraConeSettings.Fill)
            {
                imDrawListPtr.PathFillConvex(cameraConeSettings.ConeColor);
            }
            else
            {
                imDrawListPtr.PathStroke(cameraConeSettings.ConeColor, ImDrawFlags.None, 2f);
            }
        }

        // Player cone
        var playerConeSettings = configuration.cfg.Radar2DConfiguration.PlayerConeSettings;
        if (playerConeSettings.Enabled)
        {
            var playerDirection = -(playerRotation - Single.Pi / 2 + cameraRotationOffset);
            var angleOffset = playerConeSettings.RadianAngle;
            /*if (!configuration.cfg.Radar2DConfiguration.RotationLockedNorth)
            {
                tempRot += cameraRotation;
            }*/
            // if draw cone
            imDrawListPtr.PathLineTo(center);
            //var diffRight = new Vector2(50, -50);
            //diffRight = diffRight.Rotate(cameraRotation);
            //imDrawListPtr.PathLineTo(center + diffLeft);
            imDrawListPtr.PathArcTo(center, playerConeSettings.Radius, playerDirection - angleOffset, playerDirection + angleOffset, 200);
            imDrawListPtr.PathLineTo(center);
            if (configuration.cfg.Radar2DConfiguration.PlayerConeSettings.Fill)
            {
                imDrawListPtr.PathFillConvex(playerConeSettings.ConeColor);
            }
            else
            {
                imDrawListPtr.PathStroke(playerConeSettings.ConeColor, ImDrawFlags.None, 2f);
            }
        }
    }

    private void Draw2DRadarPopupSettings()
    {
        var shouldSave = false;
        if (!ImGui.BeginPopup("Radar2DPopupSettings")) return;
        shouldSave |= UiHelpers.Draw2DRadarSettings(ref configuration.cfg.Radar2DConfiguration);
        ImGui.EndPopup();
        if (shouldSave)
        {
            configuration.Save();
        }
    }

    private void DrawTag(ImDrawListPtr imDrawListPtr, IGameObject areaObject, Vector2 position, uint color)
    {
        var tag = areaObject.Name.TextValue;
        var tagTextSize = ImGui.CalcTextSize(tag);
        imDrawListPtr.AddText(
            new Vector2(position.X - tagTextSize.X / 2f, position.Y + tagTextSize.Y / 2f),
            color,
            tag);
    }
}