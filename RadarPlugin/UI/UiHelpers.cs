using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using ImGuiNET;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public static class UiHelpers
{
    public static bool Vector4ColorSelector(string label, ref uint configColor, ImGuiColorEditFlags flags = ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs)
    {
        var tempColor = ImGui.ColorConvertU32ToFloat4(configColor);
        if (!ImGui.ColorEdit4(label, ref tempColor, ImGuiColorEditFlags.NoInputs)) return false;
        configColor = ImGui.ColorConvertFloat4ToU32(tempColor);
        return true;
    }
    public static bool DrawCheckbox(string label, ref bool boxValue, string? tooltipText)
    {
        var retStatement = false;
        var tempVar = boxValue;
        if (ImGui.Checkbox(label, ref tempVar))
        {
            boxValue = tempVar;
            retStatement = true;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Will show your player character if enabled. Inherits player settings.");
        }

        return retStatement;
    }
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

    public static void LabeledHelpMarker(string label, string tooltip)
    {
        ImGuiComponents.HelpMarker(tooltip);
        ImGui.SameLine();
        ImGui.TextUnformatted(label);
        HoverTooltip(tooltip);
    }
    
    public static void HoverTooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }
    
    public static void TextURL(string name, string url, uint color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.Text(name);
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                PluginLog.Log("Opening RadarPlugin GitHub");
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }

            DrawUnderline(ImGui.GetColorU32(ImGuiCol.ButtonHovered));
            ImGui.BeginTooltip();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(FontAwesomeIcon.Link.ToIconString()); ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.PopFont();
            ImGui.Text(url);
            ImGui.EndTooltip();
        }
        else
        {
            DrawUnderline(ImGui.GetColorU32(ImGuiCol.Button));
        }
    }

    public static void DrawUnderline(uint color)
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        min.Y = max.Y;
        ImGui.GetWindowDrawList().AddLine(min, max, color, 1.0f);
    }
    
    public static bool GetBorderClampedVector2(
        Vector2 screenpos,
        Vector2 clampSize,
        out Vector2 clampedPos)
    {
        var mainViewport = ImGuiHelpers.MainViewport;
        var center = mainViewport.GetCenter();
        var vector2_1 = mainViewport.Pos + clampSize;
        var vector2_2 = mainViewport.Pos + new Vector2((mainViewport).Size.X - clampSize.X, clampSize.Y);
        var vector2_3 = mainViewport.Pos + new Vector2(clampSize.X, (mainViewport).Size.Y - clampSize.Y);
        var vector2_4 = mainViewport.Pos + (mainViewport).Size -
                        clampSize;
        bool lines_intersect;
        bool segmentsIntersect1;
        Vector2 intersection1;
        Vector2 vector2_5;
        Vector2 vector2_6;
        FindIntersection(vector2_1, vector2_2, center, screenpos, out lines_intersect,
            out segmentsIntersect1, out intersection1, out vector2_5, out vector2_6);
        bool segmentsIntersect2;
        Vector2 intersection2;
        FindIntersection(vector2_2, vector2_4, center, screenpos, out lines_intersect,
            out segmentsIntersect2, out intersection2, out vector2_6, out vector2_5);
        bool segmentsIntersect3;
        Vector2 intersection3;
        FindIntersection(vector2_4, vector2_3, center, screenpos, out lines_intersect,
            out segmentsIntersect3, out intersection3, out vector2_5, out vector2_6);
        bool segmentsIntersect4;
        Vector2 intersection4;
        FindIntersection(vector2_3, vector2_1, center, screenpos, out lines_intersect,
            out segmentsIntersect4, out intersection4, out vector2_6, out vector2_5);
        if (segmentsIntersect1)
            clampedPos = intersection1;
        else if (segmentsIntersect2)
            clampedPos = intersection2;
        else if (segmentsIntersect3)
        {
            clampedPos = intersection3;
        }
        else
        {
            if (!segmentsIntersect4)
            {
                clampedPos = Vector2.Zero;
                return false;
            }

            clampedPos = intersection4;
        }

        return true;
    }

    private static void FindIntersection(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        out bool lines_intersect,
        out bool segmentsIntersect,
        out Vector2 intersection,
        out Vector2 closeP1,
        out Vector2 closeP2)
    {
        var num1 = p2.X - p1.X;
        var num2 = p2.Y - p1.Y;
        var num3 = p4.X - p3.X;
        var num4 = p4.Y - p3.Y;
        var num5 = (float)((double)num2 * (double)num3 - (double)num1 * (double)num4);
        var f = (float)(((double)p1.X - (double)p3.X) * (double)num4 + ((double)p3.Y - (double)p1.Y) * (double)num3) /
                  num5;
        if (float.IsInfinity(f))
        {
            lines_intersect = false;
            segmentsIntersect = false;
            intersection = new Vector2(float.NaN, float.NaN);
            closeP1 = new Vector2(float.NaN, float.NaN);
            closeP2 = new Vector2(float.NaN, float.NaN);
        }
        else
        {
            lines_intersect = true;
            float num6 =
                (float)((((double)p3.X - (double)p1.X) * (double)num2 + ((double)p1.Y - (double)p3.Y) * (double)num1) /
                        (0.0 - (double)num5));
            intersection = new Vector2(p1.X + num1 * f, p1.Y + num2 * f);
            segmentsIntersect = (double)f >= 0.0 && (double)f <= 1.0 && (double)num6 >= 0.0 && (double)num6 <= 1.0;
            if ((double)f < 0.0)
                f = 0.0f;
            else if ((double)f > 1.0)
                f = 1f;
            if ((double)num6 < 0.0)
                num6 = 0.0f;
            else if ((double)num6 > 1.0)
                num6 = 1f;
            closeP1 = new Vector2(p1.X + num1 * f, p1.Y + num2 * f);
            closeP2 = new Vector2(p3.X + num3 * num6, p3.Y + num4 * num6);
        }
    }

    public static Vector2 Rotate(this Vector2 vin, float rotation, Vector2 origin) =>
        origin + (vin - origin).Rotate(rotation);

    public static Vector2 Rotate(this Vector2 vin, float rotation) =>
        vin.Rotate(new Vector2((float)Math.Sin((double)rotation), (float)Math.Cos((double)rotation)));

    public static Vector2 Rotate(this Vector2 vin, Vector2 rotation, Vector2 origin) =>
        origin + (vin - origin).Rotate(rotation);

    public static Vector2 Rotate(this Vector2 vin, Vector2 rotation)
    {
        rotation = rotation.Normalize();
        return new Vector2((float)((double)rotation.Y * (double)vin.X + (double)rotation.X * (double)vin.Y),
            (float)((double)rotation.Y * (double)vin.Y - (double)rotation.X * (double)vin.X));
    }

    public static Vector2 Normalize(this Vector2 v)
    {
        float num1 = v.Length();
        if (num1 == 0)
            return v;
        float num2 = 1f / num1;
        v.X *= num2;
        v.Y *= num2;
        return v;
    }


    public static void DrawArrow(
        this ImDrawListPtr drawList,
        Vector2 pos,
        float size,
        uint color,
        uint bgcolor,
        float rotation,
        float thickness,
        float outlinethickness)
    {
        (drawList).AddPolyline(ref new Vector2[3]
        {
            pos + new Vector2((float)(0.0 - (double)size - (double)outlinethickness / 2.0),
                (float)(-0.5 * (double)size - (double)outlinethickness / 2.0)).Rotate(rotation),
            pos + new Vector2(0.0f, 0.5f * size).Rotate(rotation),
            pos + new Vector2(size + outlinethickness / 2f,
                (float)(-0.5 * (double)size - (double)outlinethickness / 2.0)).Rotate(rotation)
        }[0], 3, bgcolor, (ImDrawFlags)240, (float)((double)thickness + (double)outlinethickness));
        drawList.DrawArrow(pos, size, color, rotation, thickness);
    }

    public static void DrawArrow(
        this ImDrawListPtr drawList,
        Vector2 pos,
        float size,
        uint color,
        float rotation,
        float thickness)
    {
        (drawList).AddPolyline(ref new Vector2[3]
        {
            pos + new Vector2(0.0f - size, -0.5f * size).Rotate(rotation),
            pos + new Vector2(0.0f, 0.5f * size).Rotate(rotation),
            pos + new Vector2(size, -0.5f * size).Rotate(rotation)
        }[0], 3, color, (ImDrawFlags)240, thickness);
    }

    public static void DrawArrow(
        this ImDrawListPtr drawList,
        Vector2 pos,
        float size,
        uint color,
        uint bgcolor,
        Vector2 rotation,
        float thickness,
        float outlinethickness)
    {
        (drawList).AddPolyline(ref new Vector2[3]
        {
            pos + new Vector2((float)(0.0 - (double)size - (double)outlinethickness / 2.0),
                (float)(-0.40000000596046448 * (double)size - (double)outlinethickness / 2.0)).Rotate(rotation),
            pos + new Vector2(0.0f, 0.6f * size).Rotate(rotation),
            pos + new Vector2(size + outlinethickness / 2f,
                (float)(-0.40000000596046448 * (double)size - (double)outlinethickness / 2.0)).Rotate(rotation)
        }[0], 3, bgcolor, (ImDrawFlags)240, (float)((double)thickness + (double)outlinethickness));
        drawList.DrawArrow(pos, size, color, rotation, thickness);
    }

    public static void DrawArrow(
        this ImDrawListPtr drawList,
        Vector2 pos,
        float size,
        uint color,
        Vector2 rotation,
        float thickness)
    {
        (drawList).AddPolyline(ref new Vector2[3]
        {
            pos + new Vector2(0.0f - size, -0.4f * size).Rotate(rotation),
            pos + new Vector2(0.0f, 0.6f * size).Rotate(rotation),
            pos + new Vector2(size, -0.4f * size).Rotate(rotation)
        }[0], 3, color, (ImDrawFlags)240, thickness);
    }
}