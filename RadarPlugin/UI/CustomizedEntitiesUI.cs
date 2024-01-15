using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;

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
            ImGui.InputText("", ref currentTypedDataId, 10,
                ImGuiInputTextFlags.CharsDecimal | ImGuiInputTextFlags.CharsNoBlank);
            if (ImGui.Button("Add As Object"))
            {
                var parsed = uint.TryParse(currentTypedDataId, out var dataId);
                if (!parsed)
                {
                    ImGui.OpenPopup("FailedDataIdParsePopup");
                }
                else
                {
                    configInterface.cfg.OptionOverride.Add(dataId,
                        new Configuration.Configuration.ESPOptionMobBased(Configuration.Configuration.objectOptDefault)
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
            UiHelpers.HoverTooltip(
                "YES ITS ONLY AN OBJECT ATM,\nTHIS MIGHT BE DANGEROUS IF YOU ADD A MOB THAT ISNT A MOB.\nWill require some engineering");
            ImGui.BeginTable("customizedEntitiesTable", 8,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable);
            ImGui.TableSetupColumn("DataId");
            ImGui.TableSetupColumn("Name When Added");
            ImGui.TableSetupColumn("Name Last Seen");
            ImGui.TableSetupColumn("Time Last Seen");
            ImGui.TableSetupColumn("Enabled");
            ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.NoSort);
            ImGui.TableSetupColumn("Edit", ImGuiTableColumnFlags.NoSort);
            ImGui.TableSetupColumn("Delete", ImGuiTableColumnFlags.NoSort);

            var optionOverrideTemporary = GetSortedOptionOverride();

            ImGui.TableHeadersRow();
            var shouldSave = false;
            foreach (var x in optionOverrideTemporary)
            {
                //DataId
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Key}");
                //Names
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Value.Name}");
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Value.LastSeenName}");
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Value.UtcLastSeenTime.ToLocalTime()}");
                //Enabled
                ImGui.TableNextColumn();
                if (ImGui.Checkbox($"##{x.Key}", ref x.Value.Enabled))
                {
                    shouldSave = true;
                }

                //Color
                //shouldSave |= UiHelpers.Vector4ColorSelector($"##{x.Key}-color", ref x.Value.ColorU, ImGuiColorEditFlags.NoInputs);
                ImGui.TableNextColumn();
                var colorChange = ImGui.ColorConvertU32ToFloat4(x.Value.ColorU);
                if (ImGui.ColorEdit4($"##{x.Key}-color", ref colorChange,
                        ImGuiColorEditFlags.NoInputs))
                {
                    configInterface.cfg.OptionOverride[x.Key].ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                    shouldSave = true;
                }

                //Edit
                ImGui.TableNextColumn();
                if (ImGui.Button($"Edit##{x.Key}"))
                {
                    var optionOverride =
                        (Configuration.Configuration.ESPOption)configInterface.cfg.OptionOverride[x.Key];
                    typeConfigurator.OpenUiWithType(ref optionOverride, x.Value.Name,
                        ((Configuration.Configuration.ESPOptionMobBased)optionOverride).MobTypeValue,
                        DisplayOrigination.DeepDungeon);
                }
                // Delete
                ImGui.TableNextColumn();
                if (ImGui.Button($"Delete##{x.Key}"))
                {
                    ImGui.OpenPopup($"DeleteConfigPopup##{x.Key}");
                }
                ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
                ImGui.PushStyleColor(ImGuiCol.Border, ImGui.GetColorU32(ImGuiCol.TabActive));
                if (ImGui.BeginPopup($"DeleteConfigPopup##{x.Key}"))
                {
                    ImGui.Text($"Are you sure you want to delete the config for the mob: {x.Value.LastSeenName}?");
                    if (ImGui.Button("Yes"))
                    {
                        configInterface.cfg.OptionOverride.Remove(x.Key);
                        shouldSave = true;
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
            }
            
            if (shouldSave)
            {
                configInterface.Save();
            }

            ImGui.EndTable();
        }


        ImGui.End();
    }

    private Dictionary<uint, Configuration.Configuration.ESPOptionMobBased> GetSortedOptionOverride()
    {
        var optionOverrideTemporary = configInterface.cfg.OptionOverride;
        var sortSpecs = ImGui.TableGetSortSpecs();
        if (sortSpecs.SpecsDirty)
        {
            switch (sortSpecs.Specs.ColumnIndex)
            {
                case 0:
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderBy(x => x.Key)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderByDescending(x => x.Key)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }

                    break;
                case 1:
                    // Name When Added
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderBy(x => x.Value.Name)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderByDescending(x => x.Value.Name)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }

                    break;
                case 2:
                    // Name Last Seen
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderBy(x => x.Value.LastSeenName)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderByDescending(x => x.Value.LastSeenName)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }

                    break;
                case 3:
                    // Time Last Seen
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderBy(x => x.Value.UtcLastSeenTime)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary
                            .OrderByDescending(x => x.Value.UtcLastSeenTime)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }

                    break;
                case 4:
                    // Enabled
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderBy(x => x.Value.Enabled)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = optionOverrideTemporary.OrderByDescending(x => x.Value.Enabled)
                            .ToDictionary(x => x.Key, x => x.Value);
                    }

                    break;
            }
        }

        return optionOverrideTemporary;
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawCustomizedEntitiesMenu;
    }
}