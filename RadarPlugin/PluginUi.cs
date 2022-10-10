using System.Collections.Generic;
using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace RadarPlugin;

public class PluginUi
{
    private List<GameObject> areaObjects { get; set; }
    private ObjectTable objectTable { get; set; }
    private Configuration configuration { get; set; }
    private DalamudPluginInterface dalamudPluginInterface { get; set; }
    //private ImGuiScene.TextureWrap goatImage;

    // this extra bool exists for ImGui, since you can't ref a property
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

    // passing in the image here just for simplicity
    public PluginUi(DalamudPluginInterface dalamudPluginInterface, Configuration configuration, ObjectTable objectTable)
    {
        areaObjects = new List<GameObject>();
        this.objectTable = objectTable;
        this.configuration = configuration;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += Draw;
        this.dalamudPluginInterface.UiBuilder.OpenConfigUi += OpenUi;
    }

    public void Draw()
    {
        DrawMainWindow();
        DrawCurrentMobsWindow();
    }

    private void DrawCurrentMobsWindow()
    {
        if (!CurrentMobsVisible)
        {
            return;
        }
        
        ImGui.SetNextWindowSize(new Vector2(420, 500), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(420, 500), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin Current Mobs Menu", ref currentMobsVisible))
        {
            ImGui.BeginTable("objecttable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("Kind");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("DataID");
            ImGui.TableSetupColumn("NameID");
            ImGui.TableSetupColumn("CurrHP");
            ImGui.TableHeadersRow();
            foreach (var x in areaObjects)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{x.SubKind}");
                ImGui.TableNextColumn();
                ImGui.Text($"{x.Name}");
                ImGui.TableNextColumn();
                ImGui.Text($"{x.DataId}");
                ImGui.TableNextColumn();
                if (x is BattleNpc mob)
                {
                    ImGui.Text($"{mob.NameId}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{mob.CurrentHp}");
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

    public void OpenUi()
    {
        MainWindowVisible = true;
    }
    
    public void DrawMainWindow()
    {
        if (!MainWindowVisible)
        {
            return;
        }
        
        ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(390, 330), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref mainWindowVisible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
        {
            ImGui.Text(
                "A 3d-radar plugin. This is basically a hack please leave me alone.");
            ImGui.Spacing();
            ImGui.Text($"Plugin Enabled: {configuration.Enabled}");

            var configValue = configuration.Enabled;
            if (ImGui.Checkbox("Enable", ref configValue))
            {
                configuration.Enabled = configValue;
                configuration.Save();
            }

            var objSHow = configuration.ObjectShow;
            if (ImGui.Checkbox("Show Objects", ref objSHow))
            {
                configuration.ObjectShow = objSHow;
                configuration.Save();
            }
            
            var objHideList = configuration.UseObjectHideList;
            if (ImGui.Checkbox("Use object hide list", ref objHideList))
            {
                configuration.UseObjectHideList = objHideList;
                configuration.Save();
            }
            
            var players = configuration.ShowPlayers;
            if (ImGui.Checkbox("Show Players", ref players))
            {
                configuration.ShowPlayers = players;
                configuration.Save();
            }
            
            ImGui.Spacing();
            if (ImGui.Button("Load Current Objects"))
            {
                CurrentMobsVisible = true;
                areaObjects.Clear();
                areaObjects.AddRange(objectTable);
            }
        }

        ImGui.End();
    }
}