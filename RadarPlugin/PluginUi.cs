using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace RadarPlugin;

public class PluginUi : IDisposable
{
    private List<GameObject> areaObjects { get; set; }
    private GameObject localObject { get; set; }
    private ObjectTable objectTable { get; set; }
    private Configuration configuration { get; set; }
    private DalamudPluginInterface dalamudPluginInterface { get; set; }

    private bool mainWindowVisible;

    public bool MainWindowVisible
    {
        get { return mainWindowVisible; }
        set { mainWindowVisible = value; }
    }

    private bool currentMobsVisible;

    public bool CurrentMobsVisible
    {
        get { return currentMobsVisible; }
        set { currentMobsVisible = value; }
    }
    
    private bool mobEditVisible;

    public bool MobEditVisible
    {
        get { return mobEditVisible; }
        set { mobEditVisible = value; }
    }

    public PluginUi(DalamudPluginInterface dalamudPluginInterface, Configuration configuration, ObjectTable objectTable)
    {
        areaObjects = new List<GameObject>();
        this.objectTable = objectTable;
        this.configuration = configuration;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += Draw;
        this.dalamudPluginInterface.UiBuilder.OpenConfigUi += OpenUi;
    }

    public void Dispose()
    {
        this.dalamudPluginInterface.UiBuilder.Draw -= Draw;
        this.dalamudPluginInterface.UiBuilder.OpenConfigUi -= OpenUi;
    }
    
    public void OpenUi()
    {
        MainWindowVisible = true;
    }
    
    private void Draw()
    {
        DrawMainWindow();
        DrawCurrentMobsWindow();
        DrawMobEditWindow();
    }

    private void DrawMobEditWindow()
    {
        if (!MobEditVisible)
        {
            return;
        }

        var size = new Vector2(600, 300);
        ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Modify Mobs Window", ref mobEditVisible))
        {
            ImGui.Columns(2);
            var utilIgnored = UtilInfo.DataIdIgnoreList.Contains(localObject.DataId);
            var userIgnored = configuration.DataIdIgnoreList.Contains(localObject.DataId);
            ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() / 2);
            // Setup First column
            ImGui.Text("Information Table");
            ImGui.BeginTable("localobjecttable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Setting");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            ImGui.TableNextColumn();
            ImGui.Text("Name");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.Name}");
            ImGui.TableNextColumn();
            ImGui.Text("Data ID");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.DataId}");
            ImGui.TableNextColumn();
            ImGui.Text("Type");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.ObjectKind}");
            ImGui.EndTable();
            
            ImGui.Text("Disabled table");
            ImGui.BeginTable("disabledbylocalobjecttable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Source");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            ImGui.TableNextColumn();
            ImGui.Text("Utility");
            ImGui.TableNextColumn();
            ImGui.Text($"{utilIgnored}");
            ImGui.TableNextColumn();
            ImGui.Text("User");
            ImGui.TableNextColumn();
            ImGui.Text($"{userIgnored}");
            ImGui.TableNextColumn();
            ImGui.Text("Overall");
            ImGui.TableNextColumn();
            ImGui.Text($"{userIgnored || utilIgnored}");
            ImGui.TableNextColumn();
            ImGui.Text("Disablable?");
            ImGui.TableNextColumn();
            ImGui.Text($"{localObject.DataId != 0}");
            ImGui.EndTable();

            // Setup second column
            ImGui.NextColumn();
            ImGui.Text("You cannot disable a mod with a data id of 0");
            if (ImGui.Button($"Add to block list"))
            {
                if (!configuration.DataIdIgnoreList.Contains(localObject.DataId))
                {
                    if (localObject.DataId != 0)
                    {
                        configuration.DataIdIgnoreList.Add(localObject.DataId);
                        configuration.Save();
                    }
                }
            }
            if (ImGui.Button($"Remove from block list"))
            {
                if (configuration.DataIdIgnoreList.Contains(localObject.DataId))
                {
                    configuration.DataIdIgnoreList.Remove(localObject.DataId);
                    configuration.Save();
                }
            }
        }
        ImGui.End();
        
    }

    private void DrawCurrentMobsWindow()
    {
        if (!CurrentMobsVisible)
        {
            return;
        }

        var size = new Vector2(560, 500);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Current Mobs Menu", ref currentMobsVisible))
        {
            ImGui.BeginTable("objecttable", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Kind");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("DataID");
            ImGui.TableSetupColumn("CurrHP");
            ImGui.TableSetupColumn("Blocked");
            ImGui.TableSetupColumn("Quick Block");
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
                else if (configuration.DataIdIgnoreList.Contains(x.DataId))
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
                    var configBlocked = configuration.DataIdIgnoreList.Contains(x.DataId);
                    if (ImGui.Checkbox($"##{x.Address}", ref configBlocked))
                    {
                        if (configBlocked)
                        {
                            if (!configuration.DataIdIgnoreList.Contains(x.DataId))
                            {
                                configuration.DataIdIgnoreList.Add(x.DataId);
                            }
                        }
                        else
                        {
                            configuration.DataIdIgnoreList.Remove(x.DataId);
                        }

                        configuration.Save();
                    }
                }
                else
                {
                    ImGui.Text("O");
                }

                ImGui.TableNextColumn();
                if (ImGui.Button($"Edit##{x.Address}"))
                {
                    localObject = x;
                    MobEditVisible = true;
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

    private void DrawMainWindow()
    {
        if (!MainWindowVisible)
        {
            return;
        }

        var size = new Vector2(375, 350);
        ImGui.SetNextWindowSize(size); //, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref mainWindowVisible))
        {
            ImGui.Text(
                "A 3d-radar plugin. This is basically a hack please leave me alone.");
            ImGui.Spacing();

            ImGui.BeginTabBar("radar-settings-tabs");

            if (ImGui.BeginTabItem($"General##radar-tabs")) {
                
                var configValue = configuration.Enabled;
                if (ImGui.Checkbox("Enabled", ref configValue))
                {
                    configuration.Enabled = configValue;
                    configuration.Save();
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem($"Visibility##radar-tabs"))
            {
                var enemyShow = configuration.ShowEnemies;
                if (ImGui.Checkbox("Enemies", ref enemyShow))
                {
                    configuration.ShowEnemies = enemyShow;
                    configuration.Save();
                }
                var objShow = configuration.ShowLoot;
                if (ImGui.Checkbox("Loot", ref objShow))
                {
                    ImGui.SetTooltip("Enables showing objects on the screen.");
                    configuration.ShowLoot = objShow;
                    configuration.Save();
                }
                var players = configuration.ShowPlayers;
                if (ImGui.Checkbox("Players", ref players))
                {
                    configuration.ShowPlayers = players;
                    configuration.Save();
                }
                ImGui.Separator();
                ImGui.Text("Below this line are things that generally won't be supported");
                ImGui.Columns(2, "##visibility-column", false);
                ImGui.Spacing();
                var npc = configuration.ShowCompanion;
                if (ImGui.Checkbox("Companion", ref npc))
                {
                    configuration.ShowCompanion = npc;
                    configuration.Save();
                }
                var eventNpcs = configuration.ShowEventNpc;
                if (ImGui.Checkbox("Event NPCs", ref eventNpcs))
                {
                    configuration.ShowEventNpc = eventNpcs;
                    configuration.Save();
                }
                var events = configuration.ShowEvents;
                if (ImGui.Checkbox("Event Objects", ref events))
                {
                    configuration.ShowEvents = events;
                    configuration.Save();
                }
                var objHideList = configuration.DebugMode;
                if (ImGui.Checkbox("Debug Mode", ref objHideList))
                {
                    configuration.DebugMode = objHideList;
                    configuration.Save();
                }
                ImGui.NextColumn();
                var showAreaObjs = configuration.ShowAreaObjects;
                if (ImGui.Checkbox("Area Objects", ref showAreaObjs))
                {
                    configuration.ShowAreaObjects = showAreaObjs;
                    configuration.Save();
                }
                ImGui.EndTabItem();
                ImGui.NextColumn();
            }
            if (ImGui.BeginTabItem($"Blocking##radar-tabs"))
            {
                if (ImGui.Button("Load Current Objects"))
                {
                    PluginLog.Debug("Pulling Area Objects");
                    CurrentMobsVisible = true;
                    areaObjects.Clear();
                    areaObjects.AddRange(objectTable);
                }
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        ImGui.End();
    }
}