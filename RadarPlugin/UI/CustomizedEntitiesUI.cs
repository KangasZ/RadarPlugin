using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public class CustomizedEntitiesUI : IDisposable
{
    private bool drawCurrentMobsWindow = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly Configuration configInterface;
    private readonly RadarHelpers helpers;
    private readonly IPluginLog pluginLog;
    private readonly TypeConfigurator typeConfigurator;
    
    public CustomizedEntitiesUI(
        DalamudPluginInterface dalamudPluginInterface,
        Configuration configInterface,
        RadarHelpers helpers,
        IPluginLog pluginLog,
        TypeConfigurator typeConfigurator)
    {
        this.helpers = helpers;
        this.configInterface = configInterface;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawCustomizedEntitiesMenu;
        this.pluginLog = pluginLog;
        this.typeConfigurator = typeConfigurator;
    }

    public void ShowCustomizedEntitiesUI()
    {
        drawCurrentMobsWindow = true;
    }

    private void DrawCustomizedEntitiesMenu()
    {
        if (!drawCurrentMobsWindow) return;

        var size = new Vector2(560, 500);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Customized Entities Menu", ref drawCurrentMobsWindow))
        {

            ImGui.BeginTable("customizedEntitiesTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("DataId");
            ImGui.TableSetupColumn("Name When Added");
            ImGui.TableSetupColumn("Enabled");
            ImGui.TableSetupColumn("Color");
            ImGui.TableSetupColumn("Edit");
            ImGui.TableHeadersRow();
            foreach (var x in configInterface.cfg.OptionOverride)
            {
                //DataId
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Key}");
                //Name
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Value.Name}");
                //Enabled
                ImGui.TableNextColumn();
                if (ImGui.Checkbox($"##{x.Key}", ref x.Value.Enabled))
                {
                    configInterface.Save();
                }
                //Color
                //shouldSave |= UiHelpers.Vector4ColorSelector($"##{x.Key}-color", ref x.Value.ColorU, ImGuiColorEditFlags.NoInputs);
                var colorChange = ImGui.ColorConvertU32ToFloat4(x.Value.ColorU);
                if (ImGui.ColorEdit4($"##{x.Key}-color", ref colorChange,
                        ImGuiColorEditFlags.NoInputs))
                {
                    configInterface.cfg.OptionOverride[x.Key].ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                    configInterface.Save();
                }
                //Edit
                ImGui.TableNextColumn();
                if (ImGui.Button($"Edit##{x.Key}"))
                {
                    var optionOverride = (Configuration.ESPOption)configInterface.cfg.OptionOverride[x.Key];
                    typeConfigurator.OpenUiWithType(ref optionOverride, x.Value.Name, ((Configuration.ESPOptionMobBased)optionOverride).MobTypeValue, DisplayOrigination.DeepDungeon);
                }
            }

            ImGui.EndTable();
        }
        

        ImGui.End();
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawCustomizedEntitiesMenu;
    }
}