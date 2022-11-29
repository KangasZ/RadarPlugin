using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
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

    private void DrawMainWindow()
    {
        if (!MainWindowVisible)
        {
            return;
        }

        var size = new Vector2(405, 365);
        ImGui.SetNextWindowSize(size); //, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref mainWindowVisible, ImGuiWindowFlags.NoResize))
        {
            ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff), "Radar Plugin. This is basically a hack. Please use with caution.");
            ImGui.Spacing();
            ImGui.BeginTabBar("radar-settings-tabs");

            DrawTab("General##radar-tabs", UtilInfo.White, DrawGeneralSettings);
            DrawTab("Visibility##radar-tabs", UtilInfo.Red, DrawVisibilitySettings);
            DrawTab("3-D Settings##radar-tabs", UtilInfo.Green, Draw3DRadarSettings);
            DrawTab("Utility##radar-tabs", UtilInfo.White, DrawUtilityTab);

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    private void DrawUtilityTab()
    {
        if (ImGui.Button("Load Current Objects"))
        {
            PluginLog.Debug("Pulling Area Objects");
            CurrentMobsVisible = true;
            areaObjects.Clear();
            areaObjects.AddRange(objectTable);
        }
    }

    private void DrawGeneralSettings()
    {
        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "This is a radar plugin.\nIt is made by KangasZ for use in FFXIV.\nPlease use this with caution.");
        var configValue = configuration.cfg.Enabled;
        if (ImGui.Checkbox("Enabled", ref configValue))
        {
            configuration.cfg.Enabled = configValue;
            configuration.Save();
        }

        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "    1. Enable type in visiblity\n" +
            "    2. Set display options in settings\n" +
            "    3. Remove invisible mobs by utility tab\n");
        ImGui.TextWrapped("Note: Entities that are not on the client are not viewable. For instance, BA traps are not visible until you unveil them.");
    }

    private void Draw3DRadarSettings()
    {
        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "This menu is WIP. Odd things may occur or not be working.");
        var objectStr = "object";
        if (ImGui.CollapsingHeader($"Object Settings##radar-{objectStr}-collapsing-header"))
        {
            ImGui.BeginChild($"##{objectStr}-radar-tabs-child", new Vector2(0, 80));
            ImGui.Columns(2, $"##{objectStr}-settings-columns", false);
            var colorChange = configuration.cfg.ObjectOption.Color;
            if (ImGui.ColorEdit4($"Color##{objectStr}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
            {
                configuration.cfg.ObjectOption.Color = colorChange;
                configuration.Save();
            }

            ImGui.SameLine();
            ImGui.TextColored(configuration.cfg.ObjectOption.Color, "Curr");
            var showObjectName = configuration.cfg.ObjectOption.ShowName;
            if (ImGui.Checkbox($"Show Name##{objectStr}-settings", ref showObjectName))
            {
                configuration.cfg.ObjectOption.ShowName = showObjectName;
                configuration.Save();
            }

            var showObjectDot = configuration.cfg.ObjectOption.ShowDot;
            if (ImGui.Checkbox($"Show Dot##{objectStr}-settings", ref showObjectDot))
            {
                configuration.cfg.ObjectOption.ShowDot = showObjectDot;
                configuration.Save();
            }

            ImGui.EndChild();
        }

        var npcStr = "npc";
        if (ImGui.CollapsingHeader($"Npc Settings##radar-{npcStr}-collapsing-header"))
        {
            ImGui.BeginChild($"##{npcStr}-radar-tabs-child", new Vector2(0, 80));
            ImGui.Columns(2, $"##{npcStr}-settings-columns", false);
            var colorChange = configuration.cfg.NpcOption.Color;
            if (ImGui.ColorEdit4($"Color##{npcStr}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
            {
                configuration.cfg.NpcOption.Color = colorChange;
                configuration.Save();
            }

            ImGui.SameLine();
            ImGui.TextColored(configuration.cfg.NpcOption.Color, "Current");
            var showNpcName = configuration.cfg.NpcOption.ShowName;
            if (ImGui.Checkbox($"Show Name##{npcStr}-settings", ref showNpcName))
            {
                configuration.cfg.NpcOption.ShowName = showNpcName;
                configuration.Save();
            }

            var showNpcDot = configuration.cfg.NpcOption.ShowDot;
            if (ImGui.Checkbox($"Show Dot##{npcStr}-settings", ref showNpcDot))
            {
                configuration.cfg.NpcOption.ShowDot = showNpcDot;
                configuration.Save();
            }

            ImGui.NextColumn();
            var showNpcHealthBar = configuration.cfg.NpcOption.ShowHealthBar;
            if (ImGui.Checkbox($"Show Health Bar##{npcStr}-settings", ref showNpcHealthBar))
            {
                configuration.cfg.NpcOption.ShowHealthBar = showNpcHealthBar;
                configuration.Save();
            }

            var showNpcHealthValue = configuration.cfg.NpcOption.ShowHealthValue;
            if (ImGui.Checkbox($"Show Health Value##{npcStr}-settings", ref showNpcHealthValue))
            {
                configuration.cfg.NpcOption.ShowHealthValue = showNpcHealthValue;
                configuration.Save();
            }

            ImGui.EndChild();
        }

        var playerStr = "player";
        if (ImGui.CollapsingHeader($"Player Settings##radar-{playerStr}-collapsing-header"))
        {
            ImGui.BeginChild($"##{playerStr}-radar-tabs-child", new Vector2(0, 80));
            ImGui.Columns(2, $"##{playerStr}-settings-columns", false);
            var colorChange = configuration.cfg.PlayerOption.Color;
            if (ImGui.ColorEdit4($"Color##{playerStr}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
            {
                configuration.cfg.PlayerOption.Color = colorChange;
                configuration.Save();
            }

            ImGui.SameLine();
            ImGui.TextColored(configuration.cfg.PlayerOption.Color, "Curr");
            var showPlayerName = configuration.cfg.PlayerOption.ShowName;
            if (ImGui.Checkbox($"Show Name##{playerStr}-settings", ref showPlayerName))
            {
                configuration.cfg.PlayerOption.ShowName = showPlayerName;
                configuration.Save();
            }

            var showPlayerDot = configuration.cfg.PlayerOption.ShowDot;
            if (ImGui.Checkbox($"Show Dot##{playerStr}-settings", ref showPlayerDot))
            {
                configuration.cfg.PlayerOption.ShowDot = showPlayerDot;
                configuration.Save();
            }

            ImGui.NextColumn();
            var showPlayerHealthBar = configuration.cfg.PlayerOption.ShowHealthBar;
            if (ImGui.Checkbox($"Show Health Bar##{playerStr}-settings", ref showPlayerHealthBar))
            {
                configuration.cfg.PlayerOption.ShowHealthBar = showPlayerHealthBar;
                configuration.Save();
            }

            var showPlayerHealthValue = configuration.cfg.PlayerOption.ShowHealthValue;
            if (ImGui.Checkbox($"Show Health Value##{playerStr}-settings", ref showPlayerHealthValue))
            {
                configuration.cfg.PlayerOption.ShowHealthValue = showPlayerHealthValue;
                configuration.Save();
            }

            ImGui.EndChild();
        }
    }

    private void DrawVisibilitySettings()
    {
        var enemyShow = configuration.cfg.ShowEnemies;
        if (ImGui.Checkbox("Enemies", ref enemyShow))
        {
            configuration.cfg.ShowEnemies = enemyShow;
            configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Shows most enemies that are considered battleable");
        }

        var objShow = configuration.cfg.ShowLoot;
        if (ImGui.Checkbox("Loot", ref objShow))
        {
            ImGui.SetTooltip("Enables showing objects on the screen.");
            configuration.cfg.ShowLoot = objShow;
            configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Shows most loot. The loot classification is via the dalamud's association.");
        }

        var players = configuration.cfg.ShowPlayers;
        if (ImGui.Checkbox("Players", ref players))
        {
            configuration.cfg.ShowPlayers = players;
            configuration.Save();
        }

        var badd = configuration.cfg.ShowBaDdObjects;
        if (ImGui.Checkbox("Eureka/Deep Dungeons", ref badd))
        {
            configuration.cfg.ShowBaDdObjects = badd;
            configuration.Save();
        }

        ImGui.Separator();
        ImGui.Text("Below this line are things that generally won't be supported");
        ImGui.BeginChild("##visibilitychild");
        ImGui.Columns(2, "##visibility-column", false);
        ImGui.Spacing();
        var npc = configuration.cfg.ShowCompanion;
        if (ImGui.Checkbox("Companion", ref npc))
        {
            configuration.cfg.ShowCompanion = npc;
            configuration.Save();
        }

        var eventNpcs = configuration.cfg.ShowEventNpc;
        if (ImGui.Checkbox("Event NPCs", ref eventNpcs))
        {
            configuration.cfg.ShowEventNpc = eventNpcs;
            configuration.Save();
        }

        var events = configuration.cfg.ShowEvents;
        if (ImGui.Checkbox("Event Objects", ref events))
        {
            configuration.cfg.ShowEvents = events;
            configuration.Save();
        }

        var objHideList = configuration.cfg.DebugMode;
        if (ImGui.Checkbox("Debug Mode", ref objHideList))
        {
            configuration.cfg.DebugMode = objHideList;
            configuration.Save();
        }

        ImGui.NextColumn();
        var showAreaObjs = configuration.cfg.ShowAreaObjects;
        if (ImGui.Checkbox("Area Objects", ref showAreaObjs))
        {
            configuration.cfg.ShowAreaObjects = showAreaObjs;
            configuration.Save();
        }

        var showAetherytes = configuration.cfg.ShowAetherytes;
        if (ImGui.Checkbox("Aetherytes", ref showAetherytes))
        {
            configuration.cfg.ShowAetherytes = showAetherytes;
            configuration.Save();
        }

        ImGui.EndChild();
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
            var userIgnored = configuration.cfg.DataIdIgnoreList.Contains(localObject.DataId);
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
                if (!configuration.cfg.DataIdIgnoreList.Contains(localObject.DataId))
                {
                    if (localObject.DataId != 0)
                    {
                        configuration.cfg.DataIdIgnoreList.Add(localObject.DataId);
                        configuration.Save();
                    }
                }
            }

            if (ImGui.Button($"Remove from block list"))
            {
                if (configuration.cfg.DataIdIgnoreList.Contains(localObject.DataId))
                {
                    configuration.cfg.DataIdIgnoreList.Remove(localObject.DataId);
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
                else if (configuration.cfg.DataIdIgnoreList.Contains(x.DataId))
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
                    var configBlocked = configuration.cfg.DataIdIgnoreList.Contains(x.DataId);
                    if (ImGui.Checkbox($"##{x.Address}", ref configBlocked))
                    {
                        if (configBlocked)
                        {
                            if (!configuration.cfg.DataIdIgnoreList.Contains(x.DataId))
                            {
                                configuration.cfg.DataIdIgnoreList.Add(x.DataId);
                            }
                        }
                        else
                        {
                            configuration.cfg.DataIdIgnoreList.Remove(x.DataId);
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

    private void DrawTab(string label, uint color, Action function)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        if (ImGui.BeginTabItem(label))
        {
            ImGui.PopStyleColor();
            function();
            ImGui.EndTabItem();
        }
        else
        {
            ImGui.PopStyleColor();
        }
    }
}