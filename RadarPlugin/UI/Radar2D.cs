using System;
using System.Numerics;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;

namespace RadarPlugin.UI;

public class Radar2D : IDisposable
{
    private readonly Configuration.Configuration configuration;
    private bool configWindowVisible = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;

    public Radar2D(DalamudPluginInterface dalamudPluginInterface, Configuration.Configuration configuration)
    {
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.configuration = configuration;
        this.dalamudPluginInterface.UiBuilder.Draw += Draw2DRadar;
    }

    public void Draw2DRadar()
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

        DrawRadar();

        ImGui.End();
    }

    private void DrawRadar()
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
    }
    
    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= Draw2DRadar;
    }
}