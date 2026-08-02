using Dalamud.Bindings.ImGui;

namespace RadarPlugin.Configuration.Models.UiSettings;

public class FontSettings
{
    public bool UseCustomFont = false;
    public bool UseAxisFont = false;
    public float FontSize = ImGui.GetFontSize();
}
