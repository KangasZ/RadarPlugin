using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;
using RadarPlugin.Constants;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

namespace RadarPlugin.UI;

public unsafe class Radar2D
{
    private readonly Configuration.Configuration configuration;
    private bool configWindowVisible = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly IClientState clientState;
    
    private readonly IPluginLog pluginLog;

    public Radar2D(DalamudPluginInterface dalamudPluginInterface, Configuration.Configuration configuration, IClientState clientState, IPluginLog pluginLog)
    {
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.configuration = configuration;
        this.clientState = clientState;
        this.pluginLog = pluginLog;
    }

    public void Radar2DOnTick(IEnumerable<GameObject> gameObjects)
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
              ImGuiWindowFlags.NoBringToFrontOnFocus |
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

        ImGui.Begin("RadarPlugin2DRadar", flags);

        DrawRadar(gameObjects);

        ImGui.End();
    }

    private void DrawRadar(IEnumerable<GameObject> gameObjects)
    {
        var imDrawListPtr = ImGui.GetWindowDrawList();
        var region = ImGui.GetContentRegionAvail();
        var start = ImGui.GetWindowPos();
        var xBegin = start.X + 10;
        var yBegin = start.Y + 10;
        var xEnd = xBegin + region.X - 5;
        var yEnd = yBegin + region.Y;
        
        var regionSize = new Vector2(region.X -5 , region.Y);
        var regionOffset = new Vector2(xBegin, yBegin);
        
        imDrawListPtr.AddCircle(regionOffset, 2, ConfigConstants.Red, 60, 5);
        imDrawListPtr.AddCircle(regionOffset + regionSize, 2, ConfigConstants.Turquoise, 60, 5);
        imDrawListPtr.AddCircle(regionOffset + regionSize with { X = 0 }, 2, ConfigConstants.Blue, 60, 5);
        imDrawListPtr.AddCircle(regionOffset + regionSize with {Y = 0}, 2, ConfigConstants.Gold, 60, 5);
        var center = regionSize with {X = regionSize.X / 2, Y = regionSize.Y / 2} + regionOffset;
        imDrawListPtr.AddCircle(center, 2, ConfigConstants.Silver, 60, 5);
        imDrawListPtr.AddLine(new Vector2(center.X, regionOffset.Y), new Vector2(center.X, regionOffset.Y+regionSize.Y), 0xFFFFFFFF, 1);
        imDrawListPtr.AddLine(new Vector2(regionOffset.X, center.Y), new Vector2(regionOffset.X+regionSize.X, center.Y), 0xFFFFFFFF, 1);

        if (clientState.LocalPlayer == null) return;
        var playerPosition = clientState.LocalPlayer.Position;
        var playerRotation = clientState.LocalPlayer.Rotation;
        
        var cameraManager = CameraManager.Instance();
        var currCamera = cameraManager->CurrentCamera;
        //pluginLog.Debug($"{currCamera->LookAtVector.X}, {currCamera->LookAtVector.Y}, {currCamera->LookAtVector.Z}");
        
        Vector3 camFront = new Vector3(currCamera->ViewMatrix.M13, currCamera->ViewMatrix.M23, currCamera->ViewMatrix.M33);
        float pitch = (float)Math.Asin(camFront.Y);

        var cameraRotation = (float)Math.Atan2(-camFront.Z / Math.Cos(pitch), -camFront.X / Math.Cos(pitch)) + Single.Pi/2;
        foreach (var mob in gameObjects)
        {
            var difference = mob.Position - playerPosition;
            var diff2 = new Vector2(difference.X, difference.Z);

            // Rotate the difference vector by the negative of the camera's rotation
            var cos = Math.Cos(-cameraRotation);
            var sin = Math.Sin(-cameraRotation);
            var rotatedDiff = new Vector2((float)(diff2.X * cos - diff2.Y * sin), (float)(diff2.X * sin + diff2.Y * cos));

            var position = center + rotatedDiff;
            imDrawListPtr.AddCircle(position, 2, ConfigConstants.Red, 60, 0.5f);
        }
        
    }
}