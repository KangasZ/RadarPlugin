using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public class TypeConfigurator
{
    private Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private bool configuratorWindowVisible = false;
    private readonly RadarHelpers radarHelper;
    private const int ChildHeight = 280;

    // Current Modification
    private Configuration.ESPOption espOption;
    private string espDescription;
    private MobType mobType;

    public TypeConfigurator(DalamudPluginInterface dalamudPluginInterface, Configuration configInterface, RadarHelpers radarHelpers)
    {
        this.configInterface = configInterface;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.radarHelper = radarHelpers;
        this.dalamudPluginInterface.UiBuilder.Draw += Draw;
        this.espOption = configInterface.cfg.NpcOption;
        this.espDescription = "NPCs...";
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= Draw;
    }

    public void OpenUiWithType(ref Configuration.ESPOption espOption, string typeId, MobType mobType)
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
            DrawSettingsDetailed(espOption, espDescription, mobType);
        }

        ImGui.End();
    }

    private void DrawSettingsDetailed(Configuration.ESPOption option, string id, MobType mobType)
    {
        bool shouldSave = false;
        UiHelpers.DrawSeperator($"{id} Options", UtilInfo.Red);
        // Column 1
        ImGui.Columns(2, $"##{id}-type-settings-columns", false);

        shouldSave |= ImGui.Checkbox($"Enabled##{id}-enabled-bool", ref option.Enabled);

        shouldSave |= UiHelpers.DrawDisplayTypesEnumListBox($"Display Type##{id}", $"{id}", mobType, ref option.DisplayType);

        shouldSave |= ImGui.Checkbox($"Override Dot Size##{id}-distance-bool", ref option.DotSizeOverride);
        
        if (mobType == MobType.Player)
        {
            shouldSave |= ImGui.Checkbox($"Replace Name With Job##{id}-name-job-replacement", ref option.ReplaceWithJobName);
            
            shouldSave |= ImGui.Checkbox($"Show MP##{id}-mp-value-shown", ref option.ShowMp);
        }
        
        
        // Column 2
        ImGui.NextColumn();
        shouldSave |= UiHelpers.Vector4ColorSelector($"Color##{id}-color", ref option.ColorU);

        shouldSave |= ImGui.Checkbox($"Append Distance to Name##{id}-distance-bool", ref option.DrawDistance);
        
        if (option.DotSizeOverride)
        {
            shouldSave |= UiHelpers.DrawFloatWithResetSlider(ref option.DotSize, "", $"{id}-font-scale-default-window", UtilInfo.MinDotSize, UtilInfo.MaxDotSize,
                UtilInfo.DefaultDotSize);
        }
        else
        {
            ImGui.Text("");
        }
        
        //Reset Column
        ImGui.Columns(1);
        if (shouldSave) configInterface.Save();
    }
}