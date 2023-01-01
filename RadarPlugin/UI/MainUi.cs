using System;
using ImGuiNET;
using System.Numerics;
using Dalamud.Logging;
using Dalamud.Plugin;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public class MainUi : IDisposable
{
    private readonly Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly LocalMobsUi localMobsUi;
    private bool mainWindowVisible = false;
    private const int ChildHeight = 240;

    public MainUi(DalamudPluginInterface dalamudPluginInterface, Configuration configInterface, LocalMobsUi localMobsUi)
    {
        this.localMobsUi = localMobsUi;
        this.configInterface = configInterface;
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
        mainWindowVisible = true;
    }

    private void Draw()
    {
        DrawMainWindow();
    }

    private void DrawMainWindow()
    {
        if (!mainWindowVisible)
        {
            return;
        }

        var size = new Vector2(405, 400);
        ImGui.SetNextWindowSize(size); //, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref mainWindowVisible, ImGuiWindowFlags.NoResize))
        {
            ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
                "Radar Plugin. This is basically a hack. Please use with caution.");
            ImGui.Spacing();
            UiHelpers.DrawTabs("radar-settings-tabs",
                ("General", UtilInfo.White, DrawGeneralSettings),
                ("Visibility", UtilInfo.Red, DrawVisibilitySettings),
                ("3-D Settings", UtilInfo.Green, Draw3DRadarSettings),
                ("Utility", UtilInfo.White, DrawUtilityTab)
            );
        }

        ImGui.End();
    }

    private void DrawUtilityTab()
    {
        if (ImGui.Button("Load Current Objects"))
        {
            PluginLog.Debug("Pulling Area Objects");
            this.localMobsUi.DrawLocalMobsUi();
        }
    }

    private void DrawGeneralSettings()
    {
        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "This is made by KangasZ for use in FFXIV.");
        var configValue = configInterface.cfg.Enabled;
        if (ImGui.Checkbox("Enabled", ref configValue))
        {
            configInterface.cfg.Enabled = configValue;
            configInterface.Save();
        }

        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "    1. Use tabs to customize experience and fix invisible mobs.\n" +
            "    2. Bring bugs or feature requests up to author\n");
        ImGui.TextColored(new Vector4(0xff, 0x00, 0x00, 0xff),
            "Major new update. Please check settings and report bugs.");
        ImGui.TextWrapped(
            "Note 1: Entities to be shown are refreshed once per second. Please be mindful of this.");
        ImGui.Spacing();
        ImGui.TextWrapped(
            "Note 2: Entities that are not on the client are not viewable. For instance, deep dungeon traps are not visible until you unveil them.");
        ImGui.Spacing();
        ImGui.TextWrapped(
            "Note 2: Invisible mobs can / will be shown. Use the Utility tab to remove these. This is kinda being worked on but does not have an easy solution.");
    }

    private void Draw3DRadarSettings()
    {
        ImGui.BeginChild($"##radar-settings-tabs-child");
        UiHelpers.DrawTabs("radar-3d-settings-tabs",
            ("Object", UtilInfo.White, DrawObjectSettings),
            ("NPC", UtilInfo.White, DrawNpcSettings),
            ("Player", UtilInfo.White, DrawPlayerSettings),
            ("DeepDung", UtilInfo.White, DrawDeepDungeonSettings)
        );
        ImGui.EndChild();
    }

    private void DrawAggroCircleSettings()
    {
        var tag = "aggroradiusoptions";

        var frontColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.AggroRadiusOptions.FrontColor);
        if (ImGui.ColorEdit4($"Front##{tag}", ref frontColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.AggroRadiusOptions.FrontColor =
                ImGui.ColorConvertFloat4ToU32(frontColor) | UtilInfo.OpacityMax;
            configInterface.Save();
        }

        ImGui.SameLine();
        var rearColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.AggroRadiusOptions.RearColor);
        if (ImGui.ColorEdit4($"Rear##{tag}", ref rearColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.AggroRadiusOptions.RearColor =
                ImGui.ColorConvertFloat4ToU32(rearColor) | UtilInfo.OpacityMax;
            configInterface.Save();
        }

        var leftSideColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.AggroRadiusOptions.LeftSideColor);
        if (ImGui.ColorEdit4($"Left##{tag}", ref leftSideColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.AggroRadiusOptions.LeftSideColor =
                ImGui.ColorConvertFloat4ToU32(leftSideColor) | UtilInfo.OpacityMax;
            configInterface.Save();
        }

        ImGui.SameLine();

        var rightSideColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.AggroRadiusOptions.RightSideColor);
        if (ImGui.ColorEdit4($"Right##{tag}", ref rightSideColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.AggroRadiusOptions.RightSideColor =
                ImGui.ColorConvertFloat4ToU32(rightSideColor) | UtilInfo.OpacityMax;
            configInterface.Save();
        }


        var circleOpacity = (float)(configInterface.cfg.AggroRadiusOptions.CircleOpacity >> 24) / byte.MaxValue;
        if (ImGui.DragFloat($"Circle Opacity##{tag}", ref circleOpacity, 0.005f, 0, 1))
        {
            configInterface.cfg.AggroRadiusOptions.CircleOpacity = ((uint)(circleOpacity * 255) << 24) | 0x00FFFFFF;
            configInterface.Save();
        }

        var coneOpacity = (float)(configInterface.cfg.AggroRadiusOptions.FrontConeOpacity >> 24) / byte.MaxValue;
        if (ImGui.DragFloat($"Cone Opacity##{tag}", ref coneOpacity, 0.005f, 0, 1))
        {
            configInterface.cfg.AggroRadiusOptions.FrontConeOpacity = ((uint)(coneOpacity * 255) << 24) | 0x00FFFFFF;
            configInterface.Save();
        }
    }

    private void DrawDeepDungeonSettings()
    {
        var tag = "deepdungeonmobtypecloroptions";
        ImGui.BeginChild($"##{tag}-deep-dungeon-settings-child", new Vector2(0, ChildHeight));
        ImGui.Columns(2, $"##{tag}-settings-columns", false);
        var defaultColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.Default);
        if (ImGui.ColorEdit4($"Default##{tag}", ref defaultColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.Default = ImGui.ColorConvertFloat4ToU32(defaultColor);
            configInterface.Save();
        }

        var specialUndeadColor =
            ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.SpecialUndead);
        if (ImGui.ColorEdit4($"Special Undead##{tag}", ref specialUndeadColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.SpecialUndead =
                ImGui.ColorConvertFloat4ToU32(specialUndeadColor);
            configInterface.Save();
        }

        var auspiceColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.Auspice);
        if (ImGui.ColorEdit4($"Auspice##{tag}", ref auspiceColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.Auspice = ImGui.ColorConvertFloat4ToU32(auspiceColor);
            configInterface.Save();
        }

        var easyMobsColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.EasyMobs);
        if (ImGui.ColorEdit4($"Easy Mobs##{tag}", ref easyMobsColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.EasyMobs = ImGui.ColorConvertFloat4ToU32(easyMobsColor);
            configInterface.Save();
        }

        var trapsColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.Traps);
        if (ImGui.ColorEdit4($"Traps##{tag}", ref trapsColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.Traps = ImGui.ColorConvertFloat4ToU32(trapsColor);
            configInterface.Save();
        }

        var returnColors = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.Return);
        if (ImGui.ColorEdit4($"Returns##{tag}", ref returnColors, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.Return = ImGui.ColorConvertFloat4ToU32(returnColors);
            configInterface.Save();
        }

        var passageColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.Passage);
        if (ImGui.ColorEdit4($"Passages##{tag}", ref passageColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.Passage = ImGui.ColorConvertFloat4ToU32(passageColor);
            configInterface.Save();
        }

        ImGui.NextColumn();
        var goldChestColor =
            ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.GoldChest);
        if (ImGui.ColorEdit4($"Gold Chest##{tag}", ref goldChestColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.GoldChest =
                ImGui.ColorConvertFloat4ToU32(goldChestColor);
            configInterface.Save();
        }

        var silverChestColor =
            ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.SilverChest);
        if (ImGui.ColorEdit4($"Silver Chest##{tag}", ref silverChestColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.SilverChest =
                ImGui.ColorConvertFloat4ToU32(silverChestColor);
            configInterface.Save();
        }

        var bronzeChestColor =
            ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.BronzeChest);
        if (ImGui.ColorEdit4($"Bronze Chest##{tag}", ref bronzeChestColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.BronzeChest =
                ImGui.ColorConvertFloat4ToU32(bronzeChestColor);
            configInterface.Save();
        }

        var mimicColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.Mimic);
        if (ImGui.ColorEdit4($"Mimics##{tag}", ref mimicColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.Mimic = ImGui.ColorConvertFloat4ToU32(mimicColor);
            configInterface.Save();
        }

        var accursedHoardColor =
            ImGui.ColorConvertU32ToFloat4(configInterface.cfg.DeepDungeonMobTypeColorOptions.AccursedHoard);
        if (ImGui.ColorEdit4($"Accursed Hoard##{tag}", ref accursedHoardColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.DeepDungeonMobTypeColorOptions.AccursedHoard =
                ImGui.ColorConvertFloat4ToU32(accursedHoardColor);
            configInterface.Save();
        }

        ImGui.EndChild();
    }

    private void DrawPlayerSettings()
    {
        var playerStr = "player";
        ImGui.BeginChild($"##{playerStr}-radar-tabs-child", new Vector2(0, ChildHeight));
        ImGui.Columns(2, $"##{playerStr}-settings-columns", false);
        var displayType = DrawDisplayTypesEnumListBox("Display Type", $"##display-type-{playerStr}", MobType.Character,
            (int)configInterface.cfg.PlayerOption.DisplayType);
        if (displayType != DisplayTypes.Default)
        {
            configInterface.cfg.PlayerOption.DisplayType = displayType;
        }

        ImGui.NextColumn();
        var colorChange = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.PlayerOption.ColorU);
        if (ImGui.ColorEdit4($"Color##{playerStr}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.PlayerOption.ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
            configInterface.Save();
        }


        var playerDotSize = configInterface.cfg.PlayerOption.DotSize;
        if (ImGui.SliderFloat($"Dot Size##{playerStr}-settings", ref playerDotSize, UtilInfo.MinDotSize,
                UtilInfo.MaxDotSize))
        {
            configInterface.cfg.PlayerOption.DotSize = playerDotSize;
            configInterface.Save();
        }

        var showDistance = configInterface.cfg.PlayerOption.DrawDistance;
        if (ImGui.Checkbox($"Append Distance to Name##{playerStr}-distance", ref showDistance))
        {
            configInterface.cfg.PlayerOption.DrawDistance = showDistance;
            configInterface.Save();
        }

        ImGui.NextColumn();


        ImGui.EndChild();
    }

    private void DrawNpcSettings()
    {
        var npcStr = "npc";
        ImGui.BeginChild($"##{npcStr}-radar-tabs-child", new Vector2(0, ChildHeight));
        ImGui.Columns(2, $"##{npcStr}-settings-columns", false);
        var displayType = DrawDisplayTypesEnumListBox("Display Type", $"##display-type-{npcStr}", MobType.Character,
            (int)configInterface.cfg.NpcOption.DisplayType);
        if (displayType != DisplayTypes.Default)
        {
            configInterface.cfg.NpcOption.DisplayType = displayType;
        }

        ImGui.NextColumn();
        var colorChange = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.NpcOption.ColorU);
        if (ImGui.ColorEdit4($"Color##{npcStr}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.NpcOption.ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
            configInterface.Save();
        }


        var npcDotSize = configInterface.cfg.NpcOption.DotSize;
        if (ImGui.SliderFloat($"Dot Size##{npcStr}-settings", ref npcDotSize, UtilInfo.MinDotSize, UtilInfo.MaxDotSize))
        {
            configInterface.cfg.NpcOption.DotSize = npcDotSize;
            configInterface.Save();
        }

        var showDistance = configInterface.cfg.NpcOption.DrawDistance;
        if (ImGui.Checkbox($"Append Distance to Name##{npcStr}-distance", ref showDistance))
        {
            configInterface.cfg.NpcOption.DrawDistance = showDistance;
            configInterface.Save();
        }

        var showNpcAggroCircle = configInterface.cfg.NpcOption.ShowAggroCircle;
        if (ImGui.Checkbox($"Aggro Circle##{npcStr}-settings", ref showNpcAggroCircle))
        {
            configInterface.cfg.NpcOption.ShowAggroCircle = showNpcAggroCircle;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Draws aggro circle.");
        }

        var onlyShowNpcAggroCircleWhenOutOfCombat = configInterface.cfg.NpcOption.ShowAggroCircleInCombat;
        if (ImGui.Checkbox($"Aggro Circle In Combat##{npcStr}-settings", ref onlyShowNpcAggroCircleWhenOutOfCombat))
        {
            configInterface.cfg.NpcOption.ShowAggroCircleInCombat = onlyShowNpcAggroCircleWhenOutOfCombat;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "If enabled, always show aggro circle.\nIf disabled, only show aggro circle when enemy is not engaged in combat.");
        }

        if (configInterface.cfg.NpcOption.ShowAggroCircle)
        {
            DrawAggroCircleSettings();
        }

        ImGui.EndChild();
    }

    private void DrawObjectSettings()
    {
        var objectStr = "object";

        ImGui.BeginChild($"##{objectStr}-radar-tabs-child", new Vector2(0, ChildHeight));
        ImGui.Columns(2, $"##{objectStr}-settings-columns", false);

        var displayType = DrawDisplayTypesEnumListBox("Display Type", $"##display-type-{objectStr}", MobType.Object,
            (int)configInterface.cfg.ObjectOption.DisplayType);
        if (displayType != DisplayTypes.Default)
        {
            configInterface.cfg.ObjectOption.DisplayType = displayType;
        }

        ImGui.NextColumn();
        var colorChange = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.ObjectOption.ColorU);
        if (ImGui.ColorEdit4($"Color##{objectStr}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.ObjectOption.ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
            configInterface.Save();
        }

        var objectDotSize = configInterface.cfg.ObjectOption.DotSize;
        if (ImGui.SliderFloat($"Dot Size##{objectStr}-settings", ref objectDotSize, UtilInfo.MinDotSize,
                UtilInfo.MaxDotSize))
        {
            configInterface.cfg.ObjectOption.DotSize = objectDotSize;
            configInterface.Save();
        }

        var showDistance = configInterface.cfg.ObjectOption.DrawDistance;
        if (ImGui.Checkbox($"Append Distance to Name##{objectStr}-distance", ref showDistance))
        {
            configInterface.cfg.ObjectOption.DrawDistance = showDistance;
            configInterface.Save();
        }

        ImGui.EndChild();
    }

    private void DrawVisibilitySettings()
    {
        var enemyShow = configInterface.cfg.ShowEnemies;
        if (ImGui.Checkbox("Enemies", ref enemyShow))
        {
            configInterface.cfg.ShowEnemies = enemyShow;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Shows most enemies that are considered battleable");
        }

        var objShow = configInterface.cfg.ShowLoot;
        if (ImGui.Checkbox("Loot", ref objShow))
        {
            ImGui.SetTooltip("Enables showing objects on the screen.");
            configInterface.cfg.ShowLoot = objShow;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Shows most loot. The loot classification is via the dalamud's association.");
        }

        var players = configInterface.cfg.ShowPlayers;
        if (ImGui.Checkbox("Players", ref players))
        {
            configInterface.cfg.ShowPlayers = players;
            configInterface.Save();
        }

        var badd = configInterface.cfg.ShowBaDdObjects;
        if (ImGui.Checkbox("Eureka/Deep Dungeons", ref badd))
        {
            configInterface.cfg.ShowBaDdObjects = badd;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "This focuses on giving support to eureka and deep dungeons.\n" +
                "Will display things such as portals, chests, and traps.");
        }

        ImGui.Separator();
        ImGui.Text("Below this line are things that generally won't be supported");
        ImGui.BeginChild("##visibilitychild");
        ImGui.Columns(2, "##visibility-column", false);
        ImGui.Spacing();
        var npc = configInterface.cfg.ShowCompanion;
        if (ImGui.Checkbox("Companion", ref npc))
        {
            configInterface.cfg.ShowCompanion = npc;
            configInterface.Save();
        }

        var eventNpcs = configInterface.cfg.ShowEventNpc;
        if (ImGui.Checkbox("Event NPCs", ref eventNpcs))
        {
            configInterface.cfg.ShowEventNpc = eventNpcs;
            configInterface.Save();
        }

        var events = configInterface.cfg.ShowEvents;
        if (ImGui.Checkbox("Event Objects", ref events))
        {
            configInterface.cfg.ShowEvents = events;
            configInterface.Save();
        }

        var you = configInterface.cfg.ShowYOU;
        if (ImGui.Checkbox("Your Player", ref you))
        {
            configInterface.cfg.ShowYOU = you;
            configInterface.Save();
        }
        
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Will show your player character if enabled. Takes player settings.");
        }
        
        var objHideList = configInterface.cfg.DebugMode;
        if (ImGui.Checkbox("Debug Mode", ref objHideList))
        {
            configInterface.cfg.DebugMode = objHideList;
            configInterface.Save();
        }

        ImGui.NextColumn();
        var showAreaObjs = configInterface.cfg.ShowAreaObjects;
        if (ImGui.Checkbox("Area Objects", ref showAreaObjs))
        {
            configInterface.cfg.ShowAreaObjects = showAreaObjs;
            configInterface.Save();
        }

        var showAetherytes = configInterface.cfg.ShowAetherytes;
        if (ImGui.Checkbox("Aetherytes", ref showAetherytes))
        {
            configInterface.cfg.ShowAetherytes = showAetherytes;
            configInterface.Save();
        }

        var onlyVisible = configInterface.cfg.ShowOnlyVisible;
        if (ImGui.Checkbox("Only Visible", ref onlyVisible))
        {
            configInterface.cfg.ShowOnlyVisible = onlyVisible;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Show only visible mobs.\nYou probably don't want to turn this off.\nMay not remove all invisible entities currently. Use the util window.");
        }

        var showNameless = configInterface.cfg.ShowNameless;
        if (ImGui.Checkbox("Nameless", ref showNameless))
        {
            configInterface.cfg.ShowNameless = showNameless;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Show nameless mobs.\nYou probably don't want this enabled.");
        }

        ImGui.EndChild();
    }

    public DisplayTypes DrawDisplayTypesEnumListBox(string name, string id, MobType mobType, int currVal)
    {
        var val = currVal;
        ImGui.Text("Display Type");
        switch (mobType)
        {
            case MobType.Object:
                ImGui.PushItemWidth(175);
                var lb = ImGui.ListBox($"##{id}",
                    ref val,
                    new string[]
                    {
                        "Dot Only",
                        "Name Only",
                        "Dot and Name",
                    }, 3, 3);
                ImGui.PopItemWidth();

                if (lb)
                {
                    switch (val)
                    {
                        case 0:
                            return DisplayTypes.DotOnly;
                        case 1:
                            return DisplayTypes.NameOnly;
                        case 2:
                            return DisplayTypes.DotAndName;
                        default:
                            PluginLog.Error("Display Type Selected Is Wrong");
                            return DisplayTypes.Default;
                    }
                }

                break;
            case MobType.Character:
                ImGui.PushItemWidth(175);
                var lb2 = ImGui.ListBox($"##{id}",
                    ref val,
                    new string[]
                    {
                        "Dot Only",
                        "Name Only",
                        "Dot and Name",
                        "Health Bar Only",
                        "Health Bar And Value",
                        "Health Bar And Name",
                        "Health Bar, Value, And Name",
                        "Health Value Only",
                        "Health Value and Name"
                    }, 9, 9);
                ImGui.PopItemWidth();
                if (lb2)
                {
                    switch (val)
                    {
                        case 0:
                            return DisplayTypes.DotOnly;
                        case 1:
                            return DisplayTypes.NameOnly;
                        case 2:
                            return DisplayTypes.DotAndName;
                        case 3:
                            return DisplayTypes.HealthBarOnly;
                        case 4:
                            return DisplayTypes.HealthBarAndValue;
                        case 5:
                            return DisplayTypes.HealthBarAndName;
                        case 6:
                            return DisplayTypes.HealthBarAndValueAndName;
                        case 7:
                            return DisplayTypes.HealthValueOnly;
                        case 8:
                            return DisplayTypes.HealthValueAndName;
                        default:
                            PluginLog.Error("Display Type Selected Is Wrong");
                            return DisplayTypes.Default;
                    }
                }

                break;
            default:
                PluginLog.Error(
                    "Mob Type Is Wrong. This literally should never occur. Please dear god help me if it does.");
                break;
        }

        return DisplayTypes.Default;
    }
}