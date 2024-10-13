using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.UI;

public class LocalMobsUi : IDisposable
{
    private string nameFilterValue = string.Empty;
    private Regex? nameFilterRegex;
    private bool currentMobsVisible = false;
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly Configuration.Configuration configInterface;
    private readonly IObjectTable objectTable;
    private readonly MobEditUi mobEditUi;
    private readonly IPluginLog pluginLog;
    private readonly RadarModules radarModules;
    private readonly TypeConfigurator typeConfigurator;

    public LocalMobsUi(
        IDalamudPluginInterface dalamudPluginInterface,
        Configuration.Configuration configInterface,
        IObjectTable objectTable,
        MobEditUi mobEditUi,
        IPluginLog pluginLog,
        RadarModules radarModules,
        TypeConfigurator typeConfigurator)
    {
        this.mobEditUi = mobEditUi;
        this.configInterface = configInterface;
        this.objectTable = objectTable;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawCurrentMobsWindow;
        this.pluginLog = pluginLog;
        this.radarModules = radarModules;
        this.typeConfigurator = typeConfigurator;
    }

    public void DrawLocalMobsUi()
    {
        currentMobsVisible = true;
    }

    private List<IGameObject> GetCurrentMobListSorted()
    {
        // Populate the array
        var areaObjects = new List<IGameObject>();
        if (configInterface.cfg.LocalMobsUiSettings.ShowPlayers)
        {
            areaObjects.AddRange(objectTable.Where(x => x.DataId == 0));
        }

        // Don't show duplicates but show npcs
        if (configInterface.cfg.LocalMobsUiSettings is { Duplicates: false, ShowNpcs: true })
        {
            areaObjects.AddRange(objectTable.Where(x => x.DataId != 0).GroupBy(x => x.DataId).Select(x => x.First()));
        }
        else if (configInterface.cfg.LocalMobsUiSettings.ShowNpcs)
        {
            areaObjects.AddRange(objectTable.Where(x => x.DataId != 0));
        }

        // Sort the array
        IEnumerable<IGameObject> areaObjectsSorted = areaObjects;
        var sortSpecs = ImGui.TableGetSortSpecs();
        if (sortSpecs.SpecsDirty)
        {
            switch (sortSpecs.Specs.ColumnIndex)
            {
                case 0:
                    // Object Kind
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        areaObjectsSorted = areaObjects.OrderBy(x => x.ObjectKind.ToString());
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        areaObjectsSorted = areaObjects.OrderByDescending(x => x.ObjectKind.ToString());
                    }

                    break;
                case 1:
                    // Name
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        areaObjectsSorted = areaObjects.OrderBy(x => x.Name?.TextValue ?? "");
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        areaObjectsSorted = areaObjects.OrderByDescending(x => x.Name?.TextValue ?? "");
                    }

                    break;
                case 2:
                    // DataId (Account ID when its a player)
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        areaObjectsSorted = areaObjects.OrderBy(x =>
                            x.ObjectKind == ObjectKind.Player ? x.GetAccountId() : x.DataId);
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        areaObjectsSorted = areaObjects.OrderByDescending(x =>
                            x.ObjectKind == ObjectKind.Player ? x.GetAccountId() : x.DataId);
                    }

                    break;
                case 3:
                    // Unsortable
                    break;
                case 4:
                    // Custom
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        areaObjectsSorted = areaObjects.OrderBy(x =>
                        {
                            radarModules.radarConfigurationModule.TryGetOverridenParams(x,
                                out var isUsingCustomEspOption);
                            return isUsingCustomEspOption;
                        });
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        areaObjectsSorted = areaObjects.OrderByDescending(x =>
                        {
                            radarModules.radarConfigurationModule.TryGetOverridenParams(x,
                                out var isUsingCustomEspOption);
                            return isUsingCustomEspOption;
                        });
                    }

                    break;
                case 5:
                    // Custom Enabled
                    if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
                    {
                        areaObjectsSorted = areaObjects.OrderBy(x =>
                        {
                            var objectParams = radarModules.radarConfigurationModule.TryGetOverridenParams(x,
                                out var isUsingCustomEspOption);
                            if (isUsingCustomEspOption)
                                return objectParams.Enabled;
                            return false;
                        });
                    }
                    else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                    {
                        areaObjectsSorted = areaObjects.OrderByDescending(x =>
                        {
                            var objectParams = radarModules.radarConfigurationModule.TryGetOverridenParams(x,
                                out var isUsingCustomEspOption);
                            if (isUsingCustomEspOption)
                                return objectParams.Enabled;
                            return false;
                        });
                    }

                    break;
            }
        }

        // Filter the array
        if (nameFilterValue.Length != 0)
        {
            areaObjectsSorted = areaObjectsSorted.Where(x =>
            {
                var name = x.Name.TextValue;
                return nameFilterRegex?.IsMatch(name) ?? name.Contains(nameFilterValue, StringComparison.OrdinalIgnoreCase);
            });
        }

        
        return areaObjectsSorted.ToList();
    }

    private void DrawCurrentMobsWindow()
    {
        if (!currentMobsVisible) return;
        var size = new Vector2(560, 500);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Current Mobs Menu", ref currentMobsVisible))
        {
            ImGui.SameLine();
            var showPlayers = configInterface.cfg.LocalMobsUiSettings.ShowPlayers;
            if (ImGui.Checkbox("Players##localmobsui", ref showPlayers))
            {
                configInterface.cfg.LocalMobsUiSettings.ShowPlayers = showPlayers;
                configInterface.Save();
            }

            ImGui.SameLine();
            var showDuplicates = configInterface.cfg.LocalMobsUiSettings.Duplicates;
            if (ImGui.Checkbox("Duplicates##localmobsui", ref showDuplicates))
            {
                configInterface.cfg.LocalMobsUiSettings.Duplicates = showDuplicates;
                configInterface.Save();
            }

            ImGui.SameLine();
            UiHelpers.LabeledHelpMarker("", "Duplicates are mobs with the same dataId");

            ImGui.SameLine();
            var showNpcs = configInterface.cfg.LocalMobsUiSettings.ShowNpcs;
            if (ImGui.Checkbox("NPCs##localmobsui", ref showNpcs))
            {
                configInterface.cfg.LocalMobsUiSettings.ShowNpcs = showNpcs;
                configInterface.Save();
            }

            // Draw the filter

            var tmp = nameFilterValue;
            if (ImGui.InputTextWithHint("Name Filter", "Input a name filter. You can use regex", ref tmp, 256))
            {
                nameFilterValue = tmp;
                try
                {
                    nameFilterRegex = new Regex(nameFilterValue,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                catch
                {
                    nameFilterRegex = null;
                }
            }

            // draw the table

            ImGui.BeginTable("objecttable", 8,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable);
            //TODO: Sortable
            ImGui.TableSetupColumn("Kind");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Data/AccId");
            ImGui.TableSetupColumn("Configuration", ImGuiTableColumnFlags.NoSort);
            ImGui.TableSetupColumn("Custom");
            ImGui.TableSetupColumn("Enabled");
            ImGui.TableSetupColumn("Custom Color", ImGuiTableColumnFlags.NoSort);
            ImGui.TableSetupColumn("Details", ImGuiTableColumnFlags.NoSort);
            ImGui.TableHeadersRow();
            var areaObjects = GetCurrentMobListSorted();
            foreach (var x in areaObjects)
            {
                var espOption = radarModules.radarConfigurationModule.GetParams(x);
                var customEspOption =
                    radarModules.radarConfigurationModule.TryGetOverridenParams(x, out var isUsingCustomEspOption);
                //Kind
                ImGui.TableNextColumn();
                ImGui.Text($"{x.ObjectKind}");
                //Name
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Name}");
                //DataId
                ImGui.TableNextColumn();
                if (x.ObjectKind == ObjectKind.Player)
                {
                    ImGui.Text($"{x.GetAccountId()}");
                }
                else
                {
                    ImGui.Text($"{x.DataId}");
                }

                //Settings
                ImGui.TableNextColumn();
                if (ImGui.Button($"Default##{x.Address}"))
                {
                    var mobType = x.GetMobType();
                    typeConfigurator.OpenUiWithType(ref espOption,
                        x.Name.TextValue, mobType,
                        DisplayOrigination.OpenWorld);
                }

                // Use Custom Settings
                ImGui.TableNextColumn();
                if (ImGui.Checkbox($"##custom-settings-{x.Address}", ref isUsingCustomEspOption))
                {
                    configInterface.Customize(x, isUsingCustomEspOption, espOption);
                }

                if (isUsingCustomEspOption)
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"Edit##{x.Address}"))
                    {
                        var mobType = x.GetMobType();
                        typeConfigurator.OpenUiWithType(ref customEspOption,
                            x.Name.TextValue, mobType,
                            DisplayOrigination.OpenWorld);
                    }
                }

                //Quick Block
                ImGui.TableNextColumn();
                if (isUsingCustomEspOption)
                {
                    var configBlocked = customEspOption.Enabled;
                    if (ImGui.Checkbox($"##{x.Address}", ref configBlocked))
                    {
                        customEspOption.Enabled = configBlocked;
                    }
                }
                else
                {
                    UiHelpers.LabeledHelpMarker("X", "Must Enable Custom Settings First");
                }

                // Quick Color
                ImGui.TableNextColumn();

                if (isUsingCustomEspOption)
                {
                    var colorChange = ImGui.ColorConvertU32ToFloat4(customEspOption.ColorU);
                    if (ImGui.ColorEdit4($"Color##{x.Address}-color", ref colorChange,
                            ImGuiColorEditFlags.NoInputs))
                    {
                        customEspOption.ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                        configInterface.Save();
                    }
                }
                else
                {
                    UiHelpers.LabeledHelpMarker("X", "Must Enable Custom Settings First");
                }

                ImGui.TableNextColumn();
                if (ImGui.Button($"Show##{x.Address}"))
                {
                    mobEditUi.Show(x);
                }

                ImGui.TableNextRow();
            }

            //TODO: Add quick edit column
            ImGui.EndTable();
        }

        ImGui.End();
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawCurrentMobsWindow;
    }
}