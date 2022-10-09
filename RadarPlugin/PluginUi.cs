using ImGuiNET;
using System.Numerics;
using Dalamud.Plugin;

namespace RadarPlugin;

public class PluginUi
{
    private Configuration configuration { get; set; }
    private DalamudPluginInterface dalamudPluginInterface { get; set; }
    //private ImGuiScene.TextureWrap goatImage;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool visible;

    public bool Visible
    {
        get { return visible; }
        set { visible = value; }
    }

    private bool settingsVisible;

    public bool SettingsVisible
    {
        get { return settingsVisible; }
        set { settingsVisible = value; }
    }

    // passing in the image here just for simplicity
    public PluginUi(DalamudPluginInterface dalamudPluginInterface, Configuration configuration)
    {
        this.configuration = configuration;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.dalamudPluginInterface.UiBuilder.Draw += DrawUi;
        this.dalamudPluginInterface.UiBuilder.OpenConfigUi += OpenUi;
    }

    public void Draw()
    {
        // This is our only draw handler attached to UIBuilder, so it needs to be
        // able to draw any windows we might have open.
        // Each method checks its own visibility/state to ensure it only draws when
        // it actually makes sense.
        // There are other ways to do this, but it is generally best to keep the number of
        // draw delegates as low as possible.

        DrawMainWindow();
    }

    public void DrawUi()
    {
        Draw();
    }

    public void OpenUi()
    {
        Visible = true;
    }
    
    public void DrawMainWindow()
    {
        if (!Visible)
        {
            return;
        }
        
        ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(390, 330), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref visible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
        {
            ImGui.Text(
                "A 3d-radar plugin. This is basically a hack please leave me alone.");
            ImGui.Spacing();
            ImGui.Text($"Plugin Enabled: {configuration.Enabled}");

            // can't ref a property, so use a local copy
            var configValue = configuration.Enabled;
            if (ImGui.Checkbox("Enable", ref configValue))
            {
                configuration.Enabled = configValue;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                configuration.Save();
            }

            var objSHow = configuration.ObjectShow;
            if (ImGui.Checkbox("Show Objects", ref objSHow))
            {
                configuration.ObjectShow = objSHow;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                configuration.Save();
            }
            
            var objHideList = configuration.UseObjectHideList;
            if (ImGui.Checkbox("Use object hide list", ref objHideList))
            {
                configuration.UseObjectHideList = objHideList;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                configuration.Save();
            }
            
            var players = configuration.ShowPlayers;
            if (ImGui.Checkbox("Show Players", ref players))
            {
                configuration.ShowPlayers = players;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                configuration.Save();
            }
            
            ImGui.Spacing();
            if (ImGui.Button("Load Current Mobs"))
            {
                
            }
        }

        ImGui.End();
    }
}