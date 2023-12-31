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
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.UI;

public class CustomizedEntitiesUI : IDisposable
{
    private string currentTypedDataId = "";
    private bool drawCurrentMobsWindow = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly Configuration.Configuration configInterface;
    private readonly IPluginLog pluginLog;
    private readonly TypeConfigurator typeConfigurator;

    public CustomizedEntitiesUI(
        DalamudPluginInterface dalamudPluginInterface,
        Configuration.Configuration configInterface,
        IPluginLog pluginLog,
        TypeConfigurator typeConfigurator)
    {
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
            ImGui.Text("Add Customized Entity dataId:");
            ImGui.SameLine();
            ImGui.InputText("", ref currentTypedDataId, 10, ImGuiInputTextFlags.CharsDecimal | ImGuiInputTextFlags.CharsNoBlank);
            if (ImGui.Button("Add As Object"))
            {
                var parsed = uint.TryParse(currentTypedDataId, out var dataId);
                if (!parsed)
                {
                    ImGui.OpenPopup("FailedDataIdParsePopup");
                }
                else
                {
                    configInterface.cfg.OptionOverride.Add(dataId, new Configuration.Configuration.ESPOptionMobBased(Configuration.Configuration.objectOptDefault)
                    {
                        Enabled = true,
                        ColorU = ConfigConstants.White,
                        Name = "Customized Entity",
                        MobTypeValue = MobType.Object
                    });
                }
            }
            //Popup
            ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.GetColorU32(ImGuiCol.TabActive));
            if (ImGui.BeginPopup("FailedDataIdParsePopup"))
            {
                UiHelpers.TextColored("Beep Beep: You didnt enter a uint :(", ConfigConstants.Red);
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                        
                ImGui.EndPopup();
            }

            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            //Popup End
            UiHelpers.HoverTooltip("YES ITS ONLY AN OBJECT ATM,\nTHIS MIGHT BE DANGEROUS IF YOU ADD A MOB THAT ISNT A MOB.\nWill require some engineering");
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
                ImGui.TableNextColumn();
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
                    var optionOverride = (Configuration.Configuration.ESPOption)configInterface.cfg.OptionOverride[x.Key];
                    typeConfigurator.OpenUiWithType(ref optionOverride, x.Value.Name, ((Configuration.Configuration.ESPOptionMobBased)optionOverride).MobTypeValue, DisplayOrigination.DeepDungeon);
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