using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;

namespace RadarPlugin.UI;

public class LocalMobsUi : IDisposable
{
    private List<GameObject> areaObjects;
    private bool currentMobsVisible = false;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly Configuration configInterface;
    private readonly ObjectTable objectTable;
    private readonly MobEditUi mobEditUi;
    private readonly RadarHelpers helpers;

    public LocalMobsUi(
        DalamudPluginInterface dalamudPluginInterface,
        Configuration configInterface,
        ObjectTable objectTable,
        MobEditUi mobEditUi,
        RadarHelpers helpers)
    {
        areaObjects = new List<GameObject>();
        this.helpers = helpers;
        this.mobEditUi = mobEditUi;
        this.configInterface = configInterface;
        this.objectTable = objectTable;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawCurrentMobsWindow;
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

            ImGui.BeginTable("objecttable", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Kind");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("DataID");
            ImGui.TableSetupColumn("CurrHP");
            ImGui.TableSetupColumn("Blocked");
            ImGui.TableSetupColumn("Quick Block");
            ImGui.TableSetupColumn("Color");
            ImGui.TableSetupColumn("Settings");
            ImGui.TableHeadersRow();
            foreach (var x in areaObjects)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{x.ObjectKind}");
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Name}");
                ImGui.TableNextColumn();
                ImGui.Text($"{x.DataId}");
                ImGui.TableNextColumn();
                if (x is BattleNpc mob)
                {
                    ImGui.Text($"{mob.CurrentHp}");
                }

                ImGui.TableNextColumn();
                if (UtilInfo.DataIdIgnoreList.Contains(x.DataId))
                {
                    ImGui.Text($"Default");
                }
                else if (configInterface.cfg.DataIdIgnoreList.Contains(x.DataId))
                {
                    ImGui.Text("User");
                }
                else
                {
                    ImGui.Text("No");
                }

                ImGui.TableNextColumn();
                if (x.DataId != 0)
                {
                    var configBlocked = configInterface.cfg.DataIdIgnoreList.Contains(x.DataId);
                    if (ImGui.Checkbox($"##{x.Address}", ref configBlocked))
                    {
                        if (configBlocked)
                        {
                            if (!configInterface.cfg.DataIdIgnoreList.Contains(x.DataId))
                            {
                                configInterface.cfg.DataIdIgnoreList.Add(x.DataId);
                            }
                        }
                        else
                        {
                            configInterface.cfg.DataIdIgnoreList.Remove(x.DataId);
                        }

                        configInterface.Save();
                    }
                }
                else
                {
                    ImGui.Text("O");
                }

                ImGui.TableNextColumn();
                if (x.DataId != 0)
                {
                    var isCustom = configInterface.cfg.ColorOverride.ContainsKey(x.DataId);
                    if (ImGui.Checkbox($"##Enabled-{x.Address}", ref isCustom))
                    {
                        if (configInterface.cfg.ColorOverride.ContainsKey(x.DataId))
                        {
                            configInterface.cfg.ColorOverride.Remove(x.DataId);
                            configInterface.Save();
                        }
                        else
                        {
                            var color = helpers.GetColor(x);
                            configInterface.cfg.ColorOverride.Add(x.DataId, color);
                            configInterface.Save();
                        }
                    }

                    if (configInterface.cfg.ColorOverride.ContainsKey(x.DataId))
                    {
                        ImGui.SameLine();
                        var colorChange = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.ColorOverride[x.DataId]);
                        if (ImGui.ColorEdit4($"Color##{x.Address}-color", ref colorChange,
                                ImGuiColorEditFlags.NoInputs))
                        {
                            configInterface.cfg.ColorOverride[x.DataId] = ImGui.ColorConvertFloat4ToU32(colorChange);
                            configInterface.Save();
                        }
                    }
                }
                else
                {
                    ImGui.Text("Uneditable (Currently)");
                }

                ImGui.TableNextColumn();
                if (ImGui.Button($"Edit##{x.Address}"))
                {
                    mobEditUi.Show(x);
                }

                ImGui.TableNextRow();
            }

            ImGui.EndTable();
        }

        if (!currentMobsVisible)
        {
            PluginLog.Debug("Clearing Area Objects");
            areaObjects.Clear();
        }

        ImGui.End();
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= DrawCurrentMobsWindow;
    }
}