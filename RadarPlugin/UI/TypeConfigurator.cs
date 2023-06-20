using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public class TypeConfigurator
{
    private Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly LocalMobsUi localMobsUi;
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
        ImGui.Columns(2, $"##{id}-type-settings-columns", false);

        shouldSave |= ImGui.Checkbox($"Enabled##{id}-enabled-bool", ref option.Enabled);

        shouldSave |= UiHelpers.DrawDisplayTypesEnumListBox($"Display Type##{id}", $"{id}", mobType, ref option.DisplayType);

        ImGui.NextColumn();
        shouldSave |= UiHelpers.Vector4ColorSelector($"Color##{id}-color", ref option.ColorU);


        shouldSave |= ImGui.Checkbox($"Append Distance to Name##{id}-distance-bool", ref option.DrawDistance);

        ImGui.NextColumn();
        shouldSave |= ImGui.Checkbox($"Override Dot Size##{id}-distance-bool", ref option.DotSizeOverride);
        ImGui.NextColumn();
        if (option.DotSizeOverride)
        {
            shouldSave |= UiHelpers.DrawFloatWithResetSlider(ref option.DotSize, "", $"{id}-font-scale-default-window", UtilInfo.MinDotSize, UtilInfo.MaxDotSize,
                UtilInfo.DefaultDotSize);
        }
        else
        {
            ImGui.Text("");
        }

        ImGui.NextColumn();
        //todo Implement this in helpers
        ImGui.Columns(1);
        if (shouldSave) configInterface.Save();
    }
}