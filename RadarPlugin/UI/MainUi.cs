using System;
using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using RadarPlugin.Enums;

namespace RadarPlugin.UI;

public class MainUi : IDisposable
{
    private Configuration configInterface;
    private readonly DalamudPluginInterface dalamudPluginInterface;
    private readonly LocalMobsUi localMobsUi;
    private bool mainWindowVisible = false;
    private readonly ClientState clientState;
    private readonly RadarHelpers radarHelper;
    private const int ChildHeight = 280;

    public MainUi(DalamudPluginInterface dalamudPluginInterface, Configuration configInterface, LocalMobsUi localMobsUi,
        ClientState clientState, RadarHelpers radarHelpers)
    {
        this.clientState = clientState;
        this.localMobsUi = localMobsUi;
        this.configInterface = configInterface;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.radarHelper = radarHelpers;
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

        var size = new Vector2(405, 440);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref mainWindowVisible))
        {
            UiHelpers.DrawTabs("radar-settings-tabs",
                ("General", UtilInfo.White, DrawGeneralSettings),
                ("Overview", UtilInfo.Red, DrawVisibilitySettings),
                ("Overworld", UtilInfo.Green, Draw3DRadarSettings),
                ("Deep Dungeon", UtilInfo.Yellow, DrawDeepDungeonVisibilitySettings),
                ("Utility", UtilInfo.White, DrawUtilityTab)
            );
        }

        ImGui.End();
    }

    private void DrawUtilityTab()
    {
        var shouldSave = false;
        var configName = configInterface.cfg.ConfigName;
        ImGui.Text("Current Config Name:");
        shouldSave |= ImGui.InputText("", ref configInterface.cfg.ConfigName, 50);

        if (ImGui.Button("Save Current Config"))
        {
            this.configInterface.SaveCurrentConfig();
        }

        ImGui.Text("Saved Configurations:");
        var selectedConfig = configInterface.selectedConfig;
        if (ImGui.Combo("##selectedconfigcombobox", ref selectedConfig, configInterface.configs,
                configInterface.configs.Length))
        {
            if (selectedConfig < configInterface.configs.Length)
            {
                configInterface.selectedConfig = selectedConfig;
            }
        }

        if (ImGui.Button("Load Selected Config"))
        {
            this.configInterface.LoadConfig(configInterface.configs[selectedConfig]);
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Selected Config"))
        {
            this.configInterface.DeleteConfig(configInterface.configs[selectedConfig]);
        }

        ImGui.Separator();

        if (ImGui.Button("Load Current Objects Menu"))
        {
            PluginLog.Debug("Pulling Area Objects");
            this.localMobsUi.DrawLocalMobsUi();
        }

        ImGui.Separator();

        ImGui.Text($"Current Map ID: {clientState.TerritoryType}");
        ImGui.Text($"In special zone (dd/eureka?): {radarHelper.IsSpecialZone()}");

        shouldSave |= ImGui.Checkbox("Show Only Visible", ref configInterface.cfg.ShowOnlyVisible);
        UiHelpers.HoverTooltip("Show only visible mobs.\nUsually you want to keep this ON\nMay not remove all invisible entities currently. Use the local objects menu.");

        shouldSave |= ImGui.Checkbox("Nameless", ref configInterface.cfg.ShowNameless);
        UiHelpers.HoverTooltip("Show nameless mobs.\nYou probably want to keep this OFF.");

        shouldSave |= ImGui.Checkbox("Debug Mode", ref configInterface.cfg.DebugMode);
        UiHelpers.HoverTooltip("Shows literally everything no matter what. Also modifies the display string.");

        if (configInterface.cfg.DebugMode)
        {
            // Todo: Debug swap text bools
        }

        if (shouldSave) configInterface.Save();
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

        var badd = configInterface.cfg.ShowBaDdObjects;
        if (ImGui.Checkbox("Eureka/Deep Dungeons Support", ref badd))
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

        var backgroundDrawList = configInterface.cfg.UseBackgroundDrawList;
        if (ImGui.Checkbox("Use Background Draw List", ref backgroundDrawList))
        {
            configInterface.cfg.UseBackgroundDrawList = backgroundDrawList;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "This feature will use a background draw list from ImGui to render the 3d radar.\n" +
                "It will be under any other Dalamud plugin. This is the original behavior.\n" +
                "There should be practically no difference between this and normal operations");
        }

        ImGui.Text("Dot Size");
        ImGui.SameLine();
        ImGui.PushItemWidth(150);
        var objectDotSize = configInterface.cfg.DotSize;
        if (ImGui.SliderFloat("##dot-size", ref objectDotSize, UtilInfo.MinDotSize,
                UtilInfo.MaxDotSize))
        {
            configInterface.cfg.DotSize = objectDotSize;
            configInterface.Save();
        }

        ImGui.TextColored(new Vector4(0xff, 0x00, 0x00, 0xff),
            "Some big changes to plugin internals for 6.4.\nIf stuff is not working please report it!\n");
        
        ImGui.TextColored(new Vector4(0xff, 0x00, 0x00, 0xff),
            "Issues or Feedback: ");
        ImGui.SameLine();
        UiHelpers.TextURL("GitHub", "https://github.com/KangasZ/RadarPlugin", ImGui.GetColorU32(ImGuiCol.Text));
        ImGui.Indent();
        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "1. Use tabs to customize experience and fix invisible mobs.\n" +
            "2. Bring bugs or feature requests up\n");
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.TextWrapped(
            "Note 1: Entities that are not on the client are not viewable. For instance, deep dungeon traps are not visible until you unveil them.");
        ImGui.Spacing();
        ImGui.TextWrapped(
            "Note 2: Invisible mobs may be shown. Use the Utility tab to remove these.");
    }

    private void Draw3DRadarSettings()
    {
        ImGui.BeginChild($"##radar-settings-tabs-child");
        UiHelpers.DrawTabs("radar-3d-settings-tabs",
            ("Players and Npcs", UtilInfo.White, DrawPlayerNpcSettings),
            ("Objects", UtilInfo.White, DrawObjectSettings),
            ("Misc", UtilInfo.White, DrawMiscSettings)
        );
        ImGui.EndChild();
    }

    private void DrawVisibilitySettings()
    {
        UiHelpers.DrawTabs("radar-visibility-tabs",
            ("Overworld", UtilInfo.Green, DrawGeneralVisibilitySettings),
            ("Deep Dungeons", UtilInfo.Yellow, DrawDeepDungeonOverviewSettings),
            ("Additional Features", UtilInfo.White, ShowMiscSettings)
        );
    }

    private void DrawDeepDungeonOverviewSettings()
    {
        DrawSeperator($"Enemies Options", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.SpecialUndeadOption, "Special Undead");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption, "'Catch All' mobs");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.AuspiceOption, "Friendly Mobs");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.EasyMobOption, "Easy Mobs");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.MimicOption, "Mimic");

        DrawSeperator($"Loot Options", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.GoldChestOption, "Gold Chest");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.SilverChestOption, "Silver Chest");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.BronzeChestOption, "Bronze Chest");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.AccursedHoardOption, "Accursed Hoard");


        DrawSeperator($"DD Specific Options", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.TrapOption, "Traps");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.ReturnOption, "Return");
        DrawSettingsOverview(configInterface.cfg.DeepDungeonOptions.PassageOption, "Passage");
    }

    private void DrawSettingsOverview(Configuration.ESPOption espOption, string tag, string description = "")
    {
        bool shouldSave = false;
        var color = ImGui.ColorConvertU32ToFloat4(espOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-{tag}", ref color,
                ImGuiColorEditFlags.NoInputs))
        {
            espOption.ColorU = ImGui.ColorConvertFloat4ToU32(color);
            shouldSave |= true;
        }

        ImGui.SameLine();

        shouldSave |= ImGui.Checkbox($"{tag}##enabled-{tag}", ref espOption.Enabled);

        // TODO: Information icon instead of hover desc
        if (!description.IsNullOrWhitespace())
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(description);
            }
        }

        if (shouldSave) configInterface.Save();
    }

    private void ShowMiscSettings()
    {
        var id = "##miscsettings";
        ImGui.BeginChild($"{id}-child", new Vector2(0, 0));

        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, UtilInfo.Red);
        ImGui.Text("Hitbox Options");
        ImGui.PopStyleColor();
        ImGui.Separator();
        var shouldSave = false;
        shouldSave |= ImGui.Checkbox($"Show Hitbox{id}-hitbox", ref configInterface.cfg.HitboxOptions.HitboxEnabled);

        var hitboxColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.HitboxOptions.HitboxColor);
        if (ImGui.ColorEdit4($"Color{id}-hitbox-color", ref hitboxColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.HitboxOptions.HitboxColor = ImGui.ColorConvertFloat4ToU32(hitboxColor);
            configInterface.Save();
        }

        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, UtilInfo.Red);
        ImGui.Text("Aggro Radius Options");
        ImGui.PopStyleColor();
        ImGui.Separator();
        DrawAggroCircleSettings();
        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, UtilInfo.Red);
        ImGui.Text("Off Screen Objects Settings");
        ImGui.PopStyleColor();
        ImGui.Separator();

        shouldSave |= ImGui.Checkbox("Show Offscreen Objects", ref configInterface.cfg.ShowOffScreen);
        UiHelpers.HoverTooltip("Show an arrow to the offscreen enemies.");

        shouldSave |= ImGui.DragFloat($"Distance From Edge{id}", ref configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge, 0.2f, 2f, 80f);

        shouldSave |= ImGui.DragFloat($"Size{id}", ref configInterface.cfg.OffScreenObjectsOptions.Size, 0.1f, 2f, 20f);

        shouldSave |= ImGui.DragFloat($"Thickness{id}", ref configInterface.cfg.OffScreenObjectsOptions.Thickness, 0.1f, 0.4f, 20f);

        ImGui.EndChild();
        if (shouldSave) configInterface.Save();
    }

    private void DrawAggroCircleSettings()
    {
        var shouldSave = false;

        var tag = "aggroradiusoptions";

        shouldSave |= ImGui.Checkbox($"Aggro Circle##{tag}-settings", ref configInterface.cfg.AggroRadiusOptions.ShowAggroCircle);
        UiHelpers.HoverTooltip("Draws aggro circle.");

        shouldSave |= ImGui.Checkbox($"Aggro Circle In Combat##{tag}-settings", ref configInterface.cfg.AggroRadiusOptions.ShowAggroCircleInCombat);

        UiHelpers.HoverTooltip("If enabled, always show aggro circle.\nIf disabled, only show aggro circle when enemy is not engaged in combat.");

        shouldSave |= UiHelpers.Vector4ColorSelector($"Front##{tag}", ref configInterface.cfg.AggroRadiusOptions.FrontColor);
        ImGui.SameLine();
        shouldSave |= UiHelpers.Vector4ColorSelector($"Rear##{tag}", ref configInterface.cfg.AggroRadiusOptions.RearColor);

        shouldSave |= UiHelpers.Vector4ColorSelector($"Left##{tag}", ref configInterface.cfg.AggroRadiusOptions.LeftSideColor);
        ImGui.SameLine();
        shouldSave |= UiHelpers.Vector4ColorSelector($"Right##{tag}", ref configInterface.cfg.AggroRadiusOptions.RightSideColor);


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

        if (shouldSave) configInterface.Save();
    }

    private void DrawDeepDungeonVisibilitySettings()
    {
        ImGui.BeginChild($"##radar-deep-dungeon-settings-tabs-child");
        UiHelpers.DrawTabs("radar-deep-dungeons-3d-settings-tabs",
            ("Mobs", UtilInfo.White, DrawDDMobSettings),
            ("Loot", UtilInfo.White, DrawDDLootSettings),
            ("DD Specific", UtilInfo.White, DrawDDEntitiesSettings)
        );
        ImGui.EndChild();
    }

    private void DrawDDEntitiesSettings()
    {
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.TrapOption, "Traps", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.ReturnOption, "Return", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.PassageOption, "Passage", MobType.Object);
    }

    private void DrawDDLootSettings()
    {
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.GoldChestOption, "Gold Chest", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.SilverChestOption, "Silver Chest", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.BronzeChestOption, "Bronze Chest", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.AccursedHoardOption, "Accursed Hoard", MobType.Object);
    }

    private void DrawDDMobSettings()
    {
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.SpecialUndeadOption, "Special Undead",
            MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.AuspiceOption, "Friendly Mobs", MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption, "'Catch All' Mobs",
            MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.EasyMobOption, "Easy Mobs", MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.MimicOption, "Mimic", MobType.Character);
    }

    private void DrawPlayerNpcSettings()
    {
        DrawTypeSettings(configInterface.cfg.PlayerOption, "Players", MobType.Character);
        DrawTypeSettings(configInterface.cfg.NpcOption, "Enemies", MobType.Character);
        DrawTypeSettings(configInterface.cfg.CompanionOption, "Companions", MobType.Object);
        DrawTypeSettings(configInterface.cfg.EventNpcOption, "Event Npcs", MobType.Object);
        DrawTypeSettings(configInterface.cfg.RetainerOption, "Retainer", MobType.Object);
    }

    private void DrawMiscSettings()
    {
        DrawTypeSettings(configInterface.cfg.CardStandOption, "Card Stand", MobType.Object);
        DrawTypeSettings(configInterface.cfg.GatheringPointOption, "Gathering Point", MobType.Object);
        DrawTypeSettings(configInterface.cfg.MountOption, "Mount", MobType.Object);
    }

    private void DrawObjectSettings()
    {
        DrawTypeSettings(configInterface.cfg.TreasureOption, "Loot", MobType.Object);
        DrawTypeSettings(configInterface.cfg.EventObjOption, "Event Objects", MobType.Object);
        DrawTypeSettings(configInterface.cfg.AreaOption, "Area Objects", MobType.Object);
        DrawTypeSettings(configInterface.cfg.AetheryteOption, "Aetherytes", MobType.Object);
        DrawTypeSettings(configInterface.cfg.CutsceneOption, "Cutscene", MobType.Object);
    }

    private void DrawTypeSettings(Configuration.ESPOption option, string id, MobType mobType)
    {
        bool shouldSave = false;
        DrawSeperator($"{id} Options", UtilInfo.Red);
        ImGui.BeginChild($"##radar-settings-tabs-child-{id}", new Vector2(0, 78));
        ImGui.Columns(2, $"##{id}-type-settings-columns", false);

        shouldSave |= ImGui.Checkbox($"Enabled##{id}-enabled-bool", ref option.Enabled);

        var displayType = DrawDisplayTypesEnumListBox($"Display Type##{id}", $"{id}", mobType, (int)option.DisplayType);
        if (displayType != DisplayTypes.Default)
        {
            option.DisplayType = displayType;
            configInterface.Save();
        }

        ImGui.NextColumn();
        shouldSave |= UiHelpers.Vector4ColorSelector($"Color##{id}-color", ref option.ColorU);

        shouldSave |= ImGui.Checkbox($"Append Distance to Name##{id}-distance-bool", ref option.DrawDistance);
        if (shouldSave) configInterface.Save();
        ImGui.EndChild();
    }

    private void DrawGeneralVisibilitySettings()
    {
        bool shouldSave = false;
        ImGui.BeginChild($"##visiblitygeneralsettings-radar-tabs-child", new Vector2(0, 0));

        DrawSeperator("Players", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.PlayerOption, "Players");

        // Custom YOUR PLAYER that I don't want to deal with yet.
        ImGui.SameLine();
        shouldSave |= UiHelpers.DrawCheckbox("Your Player##player-settings", ref configInterface.cfg.ShowYOU,
            "Will show your player character if enabled. Inherits player settings.");
        shouldSave |= ImGui.Checkbox($"Separate Party##player-settings", ref configInterface.cfg.SeparateParty);
        UiHelpers.LabeledHelpMarker("", "This will separate everything");
        shouldSave |= ImGui.Checkbox($"Separate Friends##player-settings", ref configInterface.cfg.SeparateFriends);
        shouldSave |= ImGui.Checkbox($"Separate Alliance##player-settings", ref configInterface.cfg.SeparateAlliance);

        if (configInterface.cfg.SeparateParty)
        {
            DrawSettingsOverview(configInterface.cfg.PartyOption, "Party");
        }

        if (configInterface.cfg.SeparateAlliance)
        {
            DrawSettingsOverview(configInterface.cfg.AllianceOption, "Alliance");
        }

        if (configInterface.cfg.SeparateFriends)
        {
            DrawSettingsOverview(configInterface.cfg.FriendOption, "Friends");
        }

        DrawSeperator("Npcs", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.NpcOption, "Enemies",
            "Shows most enemies that are considered battleable");
        DrawSettingsOverview(configInterface.cfg.CompanionOption, "Companions");
        DrawSettingsOverview(configInterface.cfg.EventNpcOption, "Event NPCs");
        DrawSettingsOverview(configInterface.cfg.RetainerOption, "Retainers");

        DrawSeperator("Objects", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.TreasureOption, "Treasure",
            "Shows most loot. The loot classification is via the dalamud's association.");
        DrawSettingsOverview(configInterface.cfg.EventObjOption, "Event Objects");
        DrawSettingsOverview(configInterface.cfg.AreaOption, "Area Objects");
        DrawSettingsOverview(configInterface.cfg.AetheryteOption, "Aetherytes");
        DrawSettingsOverview(configInterface.cfg.CutsceneOption, "Cutscene Objects",
            "Shows cutscene objects. I have no idea what these are!");

        DrawSeperator("Misc", UtilInfo.Red);
        DrawSettingsOverview(configInterface.cfg.CardStandOption, "Card Stand (Island Sanctuary Nodes)",
            "Show card stand. This includes island sanctuary stuff (mostly).");
        DrawSettingsOverview(configInterface.cfg.GatheringPointOption, "Gathering Point",
            "Shows Gathering Points");
        DrawSettingsOverview(configInterface.cfg.MountOption, "Mount", "Shows mounts. Gets a little cluttered");
        ImGui.EndChild();
        if (shouldSave) configInterface.Save();
    }

    private void DrawSeperator(string text, uint color)
    {
        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.Text(text);
        ImGui.PopStyleColor();
        ImGui.Separator();
    }

    public DisplayTypes DrawDisplayTypesEnumListBox(string name, string id, MobType mobType, int currVal)
    {
        var val = currVal;
        ImGui.Text("Display Type");
        switch (mobType)
        {
            case MobType.Object:
                ImGui.PushItemWidth(175);
                var lb = ImGui.Combo($"##{id}",
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
                var lb2 = ImGui.Combo($"##{id}",
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