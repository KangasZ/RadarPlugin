using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public class CustomizedEntitiesUI : IDisposable
{
    private string nameFilterValue = string.Empty;
    private Regex? filterRegex;
    private string currentTypedDataId = "";
    private bool drawCurrentMobsWindow = false;
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly Configuration.Configuration configInterface;
    private readonly IPluginLog pluginLog;
    private readonly TypeConfigurator typeConfigurator;

    public CustomizedEntitiesUI(
        IDalamudPluginInterface dalamudPluginInterface,
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
            UiHelpers.DrawTabs("customized-entities-tabs",
                ("Mobs", ConfigConstants.White, DrawMobsMenu),
                ("Players", ConfigConstants.White, DrawPlayersMenu)
            );
        }


        ImGui.End();
    }


    private void DrawMobsMenu()
    {
        var mobsOptionTypeArray = configInterface.cfg.OptionOverride.Select(x => x.Value);
        DrawMenu(mobsOptionTypeArray, MobType.Character);
    }

    private void DrawPlayersMenu()
    {
        var playersOptionTypeArray = configInterface.cfg.PlayerOptionOverride.Select(x => x.Value);
        DrawMenu(playersOptionTypeArray, MobType.Player);
    }

    private void DrawMenu(IEnumerable<Configuration.Configuration.ESPOptionMobBased> optionOverrideIEnumerable, MobType mobType)
    {
        // Filter the array
        if (nameFilterValue.Length != 0)
        {
            optionOverrideIEnumerable = optionOverrideIEnumerable.Where(x =>
            {
                var name = x.Name;
                var nameMatch = filterRegex?.IsMatch(name) ??
                                name.Contains(nameFilterValue, StringComparison.OrdinalIgnoreCase);
                var id = x.Id;
                var idString = id.ToString();
                var idMatch = filterRegex?.IsMatch(idString) ?? idString.Contains(nameFilterValue, StringComparison.OrdinalIgnoreCase);
                return nameMatch || idMatch;
            });
        }
        var optionOverrideArray = optionOverrideIEnumerable.ToArray();
        if (mobType == MobType.Character)
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
                            MobTypeValue = MobType.Object,
                            Id = dataId
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
        }
        // Draw the filter

        var tmp = nameFilterValue;
        if (ImGui.InputTextWithHint("Name Filter", "Input a name / id filter. You can use regex", ref tmp, 256))
        {
            nameFilterValue = tmp;
            try
            {
                filterRegex = new Regex(nameFilterValue,
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch
            {
                filterRegex = null;
            }
        }
        
        ImGui.BeginTable($"customizedEntitiesTable-{mobType}", 8,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable);
        ImGui.TableSetupColumn("DataId");
        ImGui.TableSetupColumn("Name When Added");
        ImGui.TableSetupColumn("Name Last Seen");
        ImGui.TableSetupColumn("Time Last Seen");
        ImGui.TableSetupColumn("Enabled");
        ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn("Edit", ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn("Delete", ImGuiTableColumnFlags.NoSort);

        var optionOverrideTemporary =
            GetSortedOptionOverride(optionOverrideArray);

        ImGui.TableHeadersRow();
        var shouldSave = false;
        foreach (var x in optionOverrideTemporary)
        {
            //DataId
            ImGui.TableNextColumn();
            ImGui.Text($"{x.Id}");
            //Names
            ImGui.TableNextColumn();
            ImGui.Text($"{x.Name}");
            ImGui.TableNextColumn();
            ImGui.Text($"{x.LastSeenName}");
            ImGui.TableNextColumn();
            ImGui.Text($"{x.UtcLastSeenTime.ToLocalTime()}");
            //Enabled
            ImGui.TableNextColumn();
            if (ImGui.Checkbox($"##{x.Id}", ref x.Enabled))
            {
                shouldSave = true;
            }

            //Color
            //shouldSave |= UiHelpers.Vector4ColorSelector($"##{x.Key}-color", ref x.Value.ColorU, ImGuiColorEditFlags.NoInputs);
            ImGui.TableNextColumn();
            var colorChange = ImGui.ColorConvertU32ToFloat4(x.ColorU);
            if (ImGui.ColorEdit4($"##{x.Id}-color", ref colorChange,
                    ImGuiColorEditFlags.NoInputs))
            {
                if (mobType == MobType.Player)
                {
                    configInterface.cfg.PlayerOptionOverride[x.Id].ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                }
                else
                {
                    var convertedUint = Convert.ToUInt32(x.Id);
                    configInterface.cfg.OptionOverride[convertedUint].ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                }

                shouldSave = true;
            }

            //Edit
            ImGui.TableNextColumn();
            if (ImGui.Button($"Edit##{x.Id}"))
            {

                Configuration.Configuration.ESPOption optionOverride;
                if (mobType == MobType.Player)
                {
                    optionOverride = configInterface.cfg.PlayerOptionOverride[x.Id];
                }
                else
                {
                    var convertedUint = Convert.ToUInt32(x.Id);
                    optionOverride = configInterface.cfg.OptionOverride[convertedUint];
                }
                typeConfigurator.OpenUiWithType(ref optionOverride, x.Name,
                    ((Configuration.Configuration.ESPOptionMobBased)optionOverride).MobTypeValue,
                    DisplayOrigination.DeepDungeon);
            }

            // Delete
            ImGui.TableNextColumn();
            if (ImGui.Button($"Delete##{x.Id}"))
            {
                ImGui.OpenPopup($"DeleteConfigPopup##{x.Id}");
            }

            ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.GetColorU32(ImGuiCol.TabActive));
            if (ImGui.BeginPopup($"DeleteConfigPopup##{x.Id}"))
            {
                ImGui.Text($"Are you sure you want to delete the config for the mob: {x.LastSeenName}?");
                if (ImGui.Button("Yes"))
                {
                    if (mobType == MobType.Player)
                    {
                        configInterface.cfg.PlayerOptionOverride.Remove(x.Id);
                    }
                    else
                    {
                        var convertedUint = Convert.ToUInt32(x.Id);
                        configInterface.cfg.OptionOverride.Remove(convertedUint);
                    }
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

    private Configuration.Configuration.ESPOptionMobBased[] GetSortedOptionOverride(
        Configuration.Configuration.ESPOptionMobBased[] arrayToSort)
    {
        IEnumerable<Configuration.Configuration.ESPOptionMobBased> optionOverrideTemporary = arrayToSort;
        var sortSpecs = ImGui.TableGetSortSpecs();
        if (sortSpecs.SpecsDirty)
        {
            switch (sortSpecs.Specs.ColumnIndex)
            {
                case 0:
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderBy(x => x.Id);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderByDescending(x => x.Id);
                    }

                    break;
                case 1:
                    // Name When Added
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderBy(x => x.Name);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderByDescending(x => x.Name);
                    }

                    break;
                case 2:
                    // Name Last Seen
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderBy(x => x.LastSeenName);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderByDescending(x => x.LastSeenName);
                    }

                    break;
                case 3:
                    // Time Last Seen
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderBy(x => x.UtcLastSeenTime);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = arrayToSort
                            .OrderByDescending(x => x.UtcLastSeenTime);
                    }

                    break;
                case 4:
                    // Enabled
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderBy(x => x.Enabled);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        optionOverrideTemporary = arrayToSort.OrderByDescending(x => x.Enabled);
                    }

                    break;
            }
        }

        return optionOverrideTemporary.ToArray();
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawCustomizedEntitiesMenu;
    }
}