using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.RadarLogic;

namespace RadarPlugin.UI;

public class LocalMobsUi : IDisposable
{
    private List<GameObject> areaObjects;
    private bool currentMobsVisible = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly Configuration.Configuration configInterface;
    private readonly IObjectTable objectTable;
    private readonly MobEditUi mobEditUi;
    private readonly IPluginLog pluginLog;
    private readonly RadarModules radarModules;
    
    public LocalMobsUi(
        DalamudPluginInterface dalamudPluginInterface,
        Configuration.Configuration configInterface,
        IObjectTable objectTable,
        MobEditUi mobEditUi,
        IPluginLog pluginLog,
        RadarModules radarModules)
    {
        areaObjects = new List<GameObject>();
        this.mobEditUi = mobEditUi;
        this.configInterface = configInterface;
        this.objectTable = objectTable;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawCurrentMobsWindow;
        this.pluginLog = pluginLog;
        this.radarModules = radarModules;
    }

    public void DrawLocalMobsUi()
    {
        areaObjects.Clear();
        IEnumerable<GameObject> tempObjects = objectTable;
        if (configInterface.cfg.LocalMobsUiSettings.ShowPlayers)
        {
            areaObjects.AddRange(objectTable.Where(x => x.DataId == 0));
        }

        // Don't show duplicates but show npcs
        if (!configInterface.cfg.LocalMobsUiSettings.Duplicates && configInterface.cfg.LocalMobsUiSettings.ShowNpcs)
        {
            areaObjects.AddRange(objectTable.Where(x => x.DataId != 0).GroupBy(x => x.DataId).Select(x => x.First()));
        }
        else if (configInterface.cfg.LocalMobsUiSettings.ShowNpcs)
        {
            areaObjects.AddRange(objectTable.Where(x => x.DataId != 0));
        }

        currentMobsVisible = true;
    }

    private void DrawCurrentMobsWindow()
    {
        if (!currentMobsVisible) return;

        var size = new Vector2(560, 500);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Current Mobs Menu", ref currentMobsVisible))
        {
            if (ImGui.Button("Reload Mobs"))
            {
                DrawLocalMobsUi();
            }

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
            var showNpcs = configInterface.cfg.LocalMobsUiSettings.ShowNpcs;
            if (ImGui.Checkbox("NPCs##localmobsui", ref showNpcs))
            {
                configInterface.cfg.LocalMobsUiSettings.ShowNpcs = showNpcs;
                configInterface.Save();
            }

            ImGui.BeginTable("objecttable", 9, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Kind");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("DataID");
            ImGui.TableSetupColumn("CurrHP");
            ImGui.TableSetupColumn("Blocked");
            ImGui.TableSetupColumn("Settings");
            ImGui.TableSetupColumn("Use Custom Settings");
            ImGui.TableSetupColumn("Quick Block");
            ImGui.TableSetupColumn("Color");
            ImGui.TableHeadersRow();
            foreach (var x in areaObjects)
            {
                var espOption = radarModules.radarConfigurationModule.GetParams(x);
                var customEspOption = radarModules.radarConfigurationModule.GetParamsWithOverride(x);
                var isUsingCustomEspOption = espOption != customEspOption;
                //Kind
                ImGui.TableNextColumn();
                ImGui.Text($"{x.ObjectKind}");
                //Name
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Name}");
                //DataId
                ImGui.TableNextColumn();
                ImGui.Text($"{x.DataId}");
                //Current HP
                ImGui.TableNextColumn();
                if (x is BattleNpc mob)
                {
                    ImGui.Text($"{mob.CurrentHp}");
                }
                //Settings
                ImGui.TableNextColumn();
                if (ImGui.Button($"Edit##{x.Address}"))
                {
                    mobEditUi.Show(x);
                }
                // Blocked
                ImGui.TableNextColumn();
                if (MobConstants.DataIdIgnoreList.Contains(x.DataId))
                {
                    ImGui.Text($"Default Blocked");
                }
                else if (isUsingCustomEspOption)
                {
                    if (customEspOption.Enabled)
                    {
                        ImGui.Text($"User Shown");
                    }
                    else
                    {
                        ImGui.Text("User Blocked");
                    }
                }
                else
                {
                    ImGui.Text("Not Blocked");
                }
                // Use Custom Settings
                ImGui.TableNextColumn();
                if (x.DataId != 0)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.Checkbox($"##custom-settings-{x.Address}", ref isUsingCustomEspOption))
                    {
                        configInterface.CustomizeMob(x, isUsingCustomEspOption, espOption);
                    }
                }
                else
                {
                    ImGui.Text("N/A");
                }
                
                //Quick Block
                ImGui.TableNextColumn();
                if (x.DataId != 0)
                {
                    if (isUsingCustomEspOption)
                    {
                        if (configInterface.cfg.OptionOverride.TryGetValue(x.DataId, out var option))
                        {
                            var configBlocked = option.Enabled;
                            if (ImGui.Checkbox($"##{x.Address}", ref configBlocked))
                            {
                                configInterface.cfg.OptionOverride[x.DataId].Enabled = configBlocked;
                            }
                        }
                        else
                        {
                            UiHelpers.LabeledHelpMarker("!!!", "Something Weird Happened :(");
                        }
                    }
                    else
                    {
                        UiHelpers.LabeledHelpMarker("X", "Must Enable Custom Settings First");
                    }
                }
                else
                {
                    UiHelpers.LabeledHelpMarker("?", "Players are unable to be blocked");
                }
                
                // Quick Color
                ImGui.TableNextColumn();
                if (x.DataId != 0)
                {
                    if (isUsingCustomEspOption)
                    {
                        if (configInterface.cfg.OptionOverride.TryGetValue(x.DataId, out var option))
                        {
                            var colorChange = ImGui.ColorConvertU32ToFloat4(option.ColorU);
                            if (ImGui.ColorEdit4($"Color##{x.Address}-color", ref colorChange,
                                    ImGuiColorEditFlags.NoInputs))
                            {
                                configInterface.cfg.OptionOverride[x.DataId].ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
                                configInterface.Save();
                            }
                        }
                        else
                        {
                            UiHelpers.LabeledHelpMarker("!!!", "Something Weird Happened :(");
                        }
                    }
                    else
                    {
                        UiHelpers.LabeledHelpMarker("X", "Must Enable Custom Settings First");
                    }
                }
                else
                {
                    UiHelpers.LabeledHelpMarker("?", "Players are unable to be changed atm");
                }

                ImGui.TableNextRow();
            }

            //TODO: Add quick edit column
            ImGui.EndTable();
        }

        if (!currentMobsVisible)
        {
            pluginLog.Debug("Clearing Area Objects");
            areaObjects.Clear();
        }

        ImGui.End();
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawCurrentMobsWindow;
    }
}