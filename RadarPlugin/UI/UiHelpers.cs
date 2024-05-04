using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public static class UiHelpers
{
    public static void TextColored(string text, uint color)
    {
        ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(color),
            text);
    }

    public static bool DrawSettingsDetailed(Configuration.Configuration.ESPOption option, string id, MobType mobType,
        DisplayOrigination displayOrigination)
    {
        var shouldSave = false;
        UiHelpers.DrawSeperator($"{id} Options", ConfigConstants.Red);
        // Column 1
        ImGui.Columns(2, $"##{id}-type-settings-columns", false);

        shouldSave |= ImGui.Checkbox($"Enabled##{id}-enabled-bool", ref option.Enabled);

        
        ImGui.NextColumn();
        shouldSave |= UiHelpers.Vector4ColorSelector($"Color##{id}-color", ref option.ColorU);
        ImGui.Columns(1);
        ImGui.Columns(2, "", false);
        shouldSave |=
            UiHelpers.DrawDisplayTypesEnumListBox($"Display Type##{id}", $"{id}", mobType,
                ref option.DisplayTypeFlags);
        ImGui.NextColumn();
        if (ImGui.Button($"More Display Options##{id}-more-display-options"))
        {
            ImGui.OpenPopup("MoreDisplayOptionsPopup");
        }
        DrawMobDisplaySettingsPopup("MoreDisplayOptionsPopup", ref option.DisplayTypeFlags);
        ImGui.Columns(1);
        shouldSave |= ImGui.Checkbox($"Display Types 2D Override##{id}-2d-bool", ref option.Separate2DOptions);
        if (option.Separate2DOptions)
        {
            ImGui.Columns(2, "", false);
            shouldSave |=
                UiHelpers.DrawDisplayTypesEnumListBox($"Display Type 2D##{id}-2d", $"{id}-2d", mobType,
                    ref option.DisplayTypeFlags2D);
            ImGui.NextColumn();
            if (ImGui.Button($"More Display Options 2D##{id}-more-display-options"))
            {
                ImGui.OpenPopup("MoreDisplayOptionsPopup2D");
            }
        }
        DrawMobDisplaySettingsPopup("MoreDisplayOptionsPopup2D", ref option.DisplayTypeFlags2D);
        ImGui.Columns(1, "", false);
        
        ImGui.Columns(2, "", false);
        shouldSave |= ImGui.Checkbox($"Override 3D Dot Size##{id}-distance-bool", ref option.DotSizeOverride);
        if (option.DotSizeOverride)
        {
            ImGui.NextColumn();
            shouldSave |= UiHelpers.DrawFloatWithResetSlider(ref option.DotSize, "", $"{id}-font-scale-default-window",
                ConfigConstants.MinDotSize, ConfigConstants.MaxDotSize,
                ConfigConstants.DefaultDotSize);
        }
        ImGui.Columns(1);
        if (mobType == MobType.Player)
        {
            shouldSave |= ImGui.Checkbox($"Replace Name With Job##{id}-name-job-replacement",
                ref option.ReplaceWithJobName);

            shouldSave |= ImGui.Checkbox($"Show MP##{id}-mp-value-shown", ref option.ShowMp);
        }

        if (mobType == MobType.Character && displayOrigination == DisplayOrigination.OpenWorld)
        {
            shouldSave |= ImGui.Checkbox("Append Level To Name", ref option.AppendLevelToName);
        }
        
        
        //Reset Column
        ImGui.Columns(1);
        return shouldSave;
    }

    public static bool DrawMobDisplaySettingsPopup(string popupId, ref DisplayTypeFlags option)
    {
        var shouldSave = false;
        if (ImGui.BeginPopup(popupId))
        {
            var drawDot = option.HasFlag(DisplayTypeFlags.Dot);
            if (UiHelpers.DrawCheckbox($"Draw Dot##{popupId}", ref drawDot, "Draws a dot where the real hitbox is"))
            {
                option.SetFlag(DisplayTypeFlags.Dot, drawDot);
                shouldSave = true;
            }

            var drawName = option.HasFlag(DisplayTypeFlags.Name);
            if (UiHelpers.DrawCheckbox($"Draw Name##{popupId}", ref drawName, "Draws the name of the object"))
            {
                option.SetFlag(DisplayTypeFlags.Name, drawName);
                shouldSave = true;
            }

            var drawHealthCircle = option.HasFlag(DisplayTypeFlags.HealthCircle);
            if (UiHelpers.DrawCheckbox($"Draw Health Circle##{popupId}", ref drawHealthCircle,
                    "Draws a circle around the object representing health"))
            {
                option.SetFlag(DisplayTypeFlags.HealthCircle, drawHealthCircle);
                shouldSave = true;
            }

            var drawHealthValue = option.HasFlag(DisplayTypeFlags.HealthValue);
            if (UiHelpers.DrawCheckbox($"Draw Health Value##{popupId}", ref drawHealthValue,
                    "Draws the health value of the object"))
            {
                option.SetFlag(DisplayTypeFlags.HealthValue, drawHealthValue);
                shouldSave = true;
            }

            var drawDistance = option.HasFlag(DisplayTypeFlags.Distance);
            if (UiHelpers.DrawCheckbox($"Draw Distance##{popupId}", ref drawDistance, "Draws the distance to the object"))
            {
                option.SetFlag(DisplayTypeFlags.Distance, drawDistance);
                shouldSave = true;
            }

            ImGui.EndPopup();
        }

        return shouldSave;
    }
    
    public static bool DrawDotSizeSlider(ref float dotSize, string id)
    {
        return DrawFloatWithResetSlider(ref dotSize, "Dot Size", id, ConfigConstants.MinDotSize,
            ConfigConstants.MaxDotSize, ConfigConstants.DefaultDotSize);
    }

    public static bool DrawFloatWithResetSlider(ref float floatToModify, string textDiscription, string id, float min,
        float max, float defaultFloatValue, string format = "%.2f")
    {
        bool shouldSave = false;
        if (!textDiscription.IsNullOrWhitespace())
        {
            ImGui.Text(textDiscription);
            ImGui.SameLine();
        }

        ImGui.PushItemWidth(150);

        shouldSave |= ImGui.SliderFloat($"##float-slider-{id}-{textDiscription}", ref floatToModify, min, max, format);

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-{id}-{textDiscription}"))
        {
            floatToModify = defaultFloatValue;
            shouldSave = true;
        }

        ImGui.PopFont();
        UiHelpers.HoverTooltip($"Default: {defaultFloatValue.ToString(CultureInfo.InvariantCulture)}");

        return shouldSave;
    }

    public static bool DrawIntWithResetSlider(ref int floatToModify, string textDiscription, string id, int min,
        int max, int defaultFloatValue)
    {
        bool shouldSave = false;
        if (!textDiscription.IsNullOrWhitespace())
        {
            ImGui.Text(textDiscription);
            ImGui.SameLine();
        }

        ImGui.PushItemWidth(150);

        shouldSave |= ImGui.SliderInt($"##float-slider-{id}-{textDiscription}", ref floatToModify, min, max);

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-{id}-{textDiscription}"))
        {
            floatToModify = defaultFloatValue;
            shouldSave = true;
        }

        ImGui.PopFont();
        UiHelpers.HoverTooltip($"Default: {defaultFloatValue.ToString(CultureInfo.InvariantCulture)}");

        return shouldSave;
    }

    public static bool DrawDisplayTypesEnumListBox(string name, string id, MobType mobType,
        ref DisplayTypeFlags currVal)
    {
        var val = (int)currVal.ToDisplayTypes();
        if (mobType == MobType.Player)
        {
            mobType = MobType.Character;
        }

        switch (mobType)
        {
            case MobType.Object:
                ImGui.PushItemWidth(175);
                var lb = ImGui.Combo($"##{id}",
                    ref val,
                    new string[]
                    {
                        "Dot",
                        "Name",
                        "Dot + Name",
                        "Custom"
                    }, 4, 4);
                ImGui.PopItemWidth();

                if (lb)
                {
                    if (val >= 0 && val <= 2)
                    {
                        currVal = ((DisplayTypes)val).ToFlags();
                    }
                }

                return lb;
            case MobType.Character:
                ImGui.PushItemWidth(175);
                var lb2 = ImGui.Combo($"##{id}",
                    ref val,
                    new string[]
                    {
                        "Dot",
                        "Name",
                        "Dot + Name",
                        "Health Bar",
                        "Health Bar + Health Value",
                        "Name + Health Bar",
                        "Name + Health Bar + Health Value",
                        "Health Value",
                        "Name + Health Value",
                        "Custom"
                    }, 10, 10);
                ImGui.PopItemWidth();
                if (lb2)
                {
                    if (val >= 0 && val <= 8)
                    {
                        currVal = ((DisplayTypes)val).ToFlags();
                    }
                }

                return lb2;
            default:
                return false;
        }
    }

    public static void DrawSeperator(string text, uint color)
    {
        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.Text(text);
        ImGui.PopStyleColor();
        ImGui.Separator();
    }

    public static bool Vector4ColorSelector(string label, ref uint configColor,
        ImGuiColorEditFlags flags = ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs)
    {
        var tempColor = ImGui.ColorConvertU32ToFloat4(configColor);
        if (!ImGui.ColorEdit4(label, ref tempColor, ImGuiColorEditFlags.NoInputs)) return false;
        configColor = ImGui.ColorConvertFloat4ToU32(tempColor);
        return true;
    }

    public static bool DrawCheckbox(string label, ref bool boxValue, string? tooltipText = null)
    {
        var retStatement = ImGui.Checkbox(label, ref boxValue);

        if (tooltipText != null)
        {
            LabeledHelpMarker("", tooltipText);
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
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }

            DrawUnderline(ImGui.GetColorU32(ImGuiCol.ButtonHovered));
            ImGui.BeginTooltip();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(FontAwesomeIcon.Link.ToIconString());
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
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

    public static bool Draw2DRadarSettings(ref Configuration.Configuration.Radar2DConfiguration cfgRadar2DConfiguration)
    {
        var shouldSave = false;
        shouldSave |= UiHelpers.DrawCheckbox("Enabled", ref cfgRadar2DConfiguration.Enabled);
        shouldSave |= UiHelpers.DrawCheckbox("Clickthrough", ref cfgRadar2DConfiguration.Clickthrough);
        shouldSave |= UiHelpers.DrawCheckbox("Show Cross", ref cfgRadar2DConfiguration.ShowCross);
        shouldSave |= UiHelpers.Vector4ColorSelector("Cross Color", ref cfgRadar2DConfiguration.CrossColor);

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-undo-cross"))
        {
            cfgRadar2DConfiguration.CrossColor = Color.White;
            shouldSave = true;
        }

        ImGui.PopFont();
        shouldSave |= UiHelpers.DrawCheckbox("Show Radar Border",
            ref cfgRadar2DConfiguration.ShowRadarBorder);
        UiHelpers.LabeledHelpMarker("", "Doesn't work atm - Request it and I'll prioritize!");
        shouldSave |= UiHelpers.DrawCheckbox("Show Radar Background",
            ref cfgRadar2DConfiguration.ShowBackground);
        shouldSave |= UiHelpers.Vector4ColorSelector("Background Color", ref cfgRadar2DConfiguration.BackgroundColor);
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.UndoAlt.ToIconString()}##-undo"))
        {
            cfgRadar2DConfiguration.BackgroundColor = Color.BackgroundDefault;
            shouldSave = true;
        }

        ImGui.PopFont();
        shouldSave |= UiHelpers.DrawCheckbox("Show Settings", ref cfgRadar2DConfiguration.ShowSettings);
        shouldSave |= UiHelpers.DrawCheckbox("Show Scale", ref cfgRadar2DConfiguration.ShowScale);
        return shouldSave;
    }
}