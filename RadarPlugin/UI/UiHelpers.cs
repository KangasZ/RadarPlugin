using System;
using ImGuiNET;

namespace RadarPlugin.UI;

public static class UiHelpers
{
    public static void DrawTabs(string tabId, params (string label, uint color, Action function)[] tabs)
    {
        ImGui.BeginTabBar($"##{tabId}");
        foreach (var tab in tabs)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, tab.color);
            if (ImGui.BeginTabItem($"{tab.label}##{tabId}"))
            {
                ImGui.PopStyleColor();
                tab.function();
                ImGui.EndTabItem();
            }
            else
            {
                ImGui.PopStyleColor();
            }
        }
        ImGui.EndTabBar();
    }
}