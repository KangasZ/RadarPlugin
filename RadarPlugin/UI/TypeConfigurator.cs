using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.UI;

public class TypeConfigurator
{
    private Configuration.Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private bool configuratorWindowVisible = false;

    // Current Modification
    private Configuration.Configuration.ESPOption espOption;
    private string espDescription;
    private MobType mobType;
    private DisplayOrigination displayOrigination;

    public TypeConfigurator(DalamudPluginInterface dalamudPluginInterface, Configuration.Configuration configInterface)
    {
        this.configInterface = configInterface;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += Draw;
        this.espOption = configInterface.cfg.NpcOption;
        this.espDescription = "NPCs...";
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= Draw;
    }

    public void OpenUiWithType(ref Configuration.Configuration.ESPOption espOption, string typeId, MobType mobType, DisplayOrigination displayOrigination)
    {
        if (typeId == this.espDescription && configuratorWindowVisible == true)
        {
            configuratorWindowVisible = false;
        }
        else
        {
            configuratorWindowVisible = true;
            espDescription = typeId;
            this.espOption = espOption;
            this.mobType = mobType;
            this.displayOrigination = displayOrigination;
        }
    }

    private void Draw()
    {
        if (!configuratorWindowVisible)
        {
            return;
        }

        DrawConfiguratorWindow();
    }

    private void DrawConfiguratorWindow()
    {
        var size = new Vector2(400, 260);
        ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Type Configurator", ref configuratorWindowVisible))
        {
            ImGui.Text($"Currently Updating: {espDescription}");
            var shouldSave = UiHelpers.DrawSettingsDetailed(espOption, espDescription, mobType, displayOrigination);
            if (shouldSave) configInterface.Save();
        }

        ImGui.End();
    }
}