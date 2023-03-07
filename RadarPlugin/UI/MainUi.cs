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
    private const int ChildHeight = 280;

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

        var size = new Vector2(405, 440);
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Radar Plugin", ref mainWindowVisible))
        {
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


        ImGui.TextColored(new Vector4(0xff, 0xff, 0x00, 0xff),
            "    1. Use tabs to customize experience and fix invisible mobs.\n" +
            "    2. Bring bugs or feature requests up to author\n");
        ImGui.TextColored(new Vector4(0xff, 0x00, 0x00, 0xff),
            "v1.4.0.0: Another change in config structure.\nMay or may not destroy some old config.");
        ImGui.TextWrapped(
            "Note 1: Entities to be shown are refreshed once per second. Please be mindful of this.");
        ImGui.Spacing();
        ImGui.TextWrapped(
            "Note 2: Entities that are not on the client are not viewable. For instance, deep dungeon traps are not visible until you unveil them.");
        ImGui.Spacing();
        ImGui.TextWrapped(
            "Note 2: Invisible mobs may be shown. Use the Utility tab to remove these. This is kinda being worked on but does not have an easy solution.");
    }

    private void Draw3DRadarSettings()
    {
        ImGui.BeginChild($"##radar-settings-tabs-child");
        UiHelpers.DrawTabs("radar-3d-settings-tabs",
            ("Players and Npcs", UtilInfo.White, DrawPlayerNpcSettings),
            ("Objects", UtilInfo.White, DrawObjectSettings),
            ("Misc", UtilInfo.White, DrawMiscSettings),
            ("Generic", UtilInfo.White, ShowMiscSettings)
        );
        ImGui.EndChild();
    }

    private void DrawVisibilitySettings()
    {
        UiHelpers.DrawTabs("radar-visibility-tabs",
            ("General Visibility", UtilInfo.White, DrawGeneralVisibilitySettings),
            ("Deep Dungeon Visibility", UtilInfo.Red, DrawDeepDungeonVisibilitySettings),
            ("Advanced Visibility", UtilInfo.White, DrawAdvancedVisibilitySettings)
        );
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
        var hitboxEnabled = configInterface.cfg.HitboxOptions.HitboxEnabled;
        if (ImGui.Checkbox($"Show Hitbox{id}-hitbox", ref hitboxEnabled))
        {
            configInterface.cfg.HitboxOptions.HitboxEnabled = hitboxEnabled;
            configInterface.Save();
        }

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
        var showOffScreen = configInterface.cfg.ShowOffScreen;
        if (ImGui.Checkbox("Show Offscreen Objects", ref showOffScreen))
        {
            configInterface.cfg.ShowOffScreen = showOffScreen;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Show an arrow to the offscreen enemies.");
        }

        var distanceFromEdge = configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge;
        if (ImGui.DragFloat($"Distance From Edge{id}", ref distanceFromEdge, 0.2f, 2f, 80f))
        {
            configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge = distanceFromEdge;
            configInterface.Save();
        }

        var size = configInterface.cfg.OffScreenObjectsOptions.Size;
        if (ImGui.DragFloat($"Size{id}", ref size, 0.1f, 2f, 20f))
        {
            configInterface.cfg.OffScreenObjectsOptions.Size = size;
            configInterface.Save();
        }

        var thickness = configInterface.cfg.OffScreenObjectsOptions.Thickness;
        if (ImGui.DragFloat($"Thickness{id}", ref thickness, 0.1f, 0.4f, 20f))
        {
            configInterface.cfg.OffScreenObjectsOptions.Thickness = thickness;
            configInterface.Save();
        }

        ImGui.EndChild();
    }

    private void DrawAggroCircleSettings()
    {
        var tag = "aggroradiusoptions";

        var showNpcAggroCircle = configInterface.cfg.NpcOption.ShowAggroCircle;
        if (ImGui.Checkbox($"Aggro Circle##{tag}-settings", ref showNpcAggroCircle))
        {
            configInterface.cfg.NpcOption.ShowAggroCircle = showNpcAggroCircle;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Draws aggro circle.");
        }

        var onlyShowNpcAggroCircleWhenOutOfCombat = configInterface.cfg.NpcOption.ShowAggroCircleInCombat;
        if (ImGui.Checkbox($"Aggro Circle In Combat##{tag}-settings", ref onlyShowNpcAggroCircleWhenOutOfCombat))
        {
            configInterface.cfg.NpcOption.ShowAggroCircleInCombat = onlyShowNpcAggroCircleWhenOutOfCombat;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "If enabled, always show aggro circle.\nIf disabled, only show aggro circle when enemy is not engaged in combat.");
        }

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

    private void DrawDeepDungeonVisibilitySettings()
    {
        var tag = "deepdungeonmobtypecloroptions";
        ImGui.BeginChild($"##{tag}-deep-dungeon-settings-child", new Vector2(0, ChildHeight));
        ImGui.Columns(2, $"##{tag}-settings-columns", false);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.SpecialUndeadOption, "Special Undead", MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.AuspiceOption, "Auspice", MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.EasyMobOption, "Easy Mobs", MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.TrapOption, "Traps", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.ReturnOption, "Return", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.PassageOption, "Passage", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.GoldChestOption, "Gold Chest", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.SilverChestOption, "Silver Chest", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.BronzeChestOption, "Bronze Chest", MobType.Object);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.MimicOption, "Mimic", MobType.Character);
        DrawTypeSettings(configInterface.cfg.DeepDungeonOptions.AccursedHoardOption, "Accursed Hoard", MobType.Object);
        ImGui.EndChild();
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
        DrawSeperator($"{id} Options", UtilInfo.Red);
        ImGui.BeginChild($"##radar-settings-tabs-child-{id}", new Vector2(0, 75));
        ImGui.Columns(2, $"##{id}-type-settings-columns", false);
        
        var colorChange = ImGui.ColorConvertU32ToFloat4(option.ColorU);
        if (ImGui.ColorEdit4($"Color##{id}-color", ref colorChange, ImGuiColorEditFlags.NoInputs))
        {
            option.ColorU = ImGui.ColorConvertFloat4ToU32(colorChange);
            configInterface.Save();
        }
        
        var displayType = DrawDisplayTypesEnumListBox($"Display Type##{id}", $"{id}", mobType,
            (int)option.DisplayType);
        if (displayType != DisplayTypes.Default)
        {
            option.DisplayType = displayType;
            configInterface.Save();
        }
        
        ImGui.NextColumn();
        var objectDotSize = option.DotSize;
        if (ImGui.SliderFloat($"Dot Size##{id}-dot-size", ref objectDotSize, UtilInfo.MinDotSize,
                UtilInfo.MaxDotSize))
        {
            option.DotSize = objectDotSize;
            configInterface.Save();
        }


        var showDistance = option.DrawDistance;
        if (ImGui.Checkbox($"Append Distance to Name##{id}-distance-bool", ref showDistance))
        {
            option.DrawDistance = showDistance;
            configInterface.Save();
        }
        ImGui.EndChild();
    }

    private void DrawAdvancedVisibilitySettings()
    {
        ImGui.TextWrapped(
            "More Advanced Settings. Unless you are a developer or know what you're doing, this menu will likely be useless.");
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

        var objHideList = configInterface.cfg.DebugMode;
        if (ImGui.Checkbox("Debug Mode", ref objHideList))
        {
            configInterface.cfg.DebugMode = objHideList;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Shows literally everything no matter what. Also modifies the display string.");
        }
    }

    private void DrawGeneralVisibilitySettings()
    {
        ImGui.BeginChild($"##visiblitygeneralsettings-radar-tabs-child", new Vector2(0, 0));


        DrawSeperator("Players and Npcs", UtilInfo.Red);

        var playersColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.PlayerOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-players", ref playersColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.PlayerOption.ColorU = ImGui.ColorConvertFloat4ToU32(playersColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var players = configInterface.cfg.ShowPlayers;
        if (ImGui.Checkbox("Players", ref players))
        {
            configInterface.cfg.ShowPlayers = players;
            configInterface.Save();
        }

        ImGui.SameLine();
        var you = configInterface.cfg.ShowYOU;
        if (ImGui.Checkbox("Your Player", ref you))
        {
            configInterface.cfg.ShowYOU = you;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Will show your player character if enabled. Inherits player settings.");
        }


        var npcColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.NpcOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-enemy-color", ref npcColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.NpcOption.ColorU = ImGui.ColorConvertFloat4ToU32(npcColor);
            configInterface.Save();
        }

        ImGui.SameLine();
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

        var companionsColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.CompanionOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-companions", ref companionsColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.CompanionOption.ColorU = ImGui.ColorConvertFloat4ToU32(companionsColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var npc = configInterface.cfg.ShowCompanion;
        if (ImGui.Checkbox("Companions", ref npc))
        {
            configInterface.cfg.ShowCompanion = npc;
            configInterface.Save();
        }


        var eventNpcsColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.EventNpcOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-eventnpcs", ref eventNpcsColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.EventNpcOption.ColorU = ImGui.ColorConvertFloat4ToU32(eventNpcsColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var eventNpcs = configInterface.cfg.ShowEventNpc;
        if (ImGui.Checkbox("Event NPCs", ref eventNpcs))
        {
            configInterface.cfg.ShowEventNpc = eventNpcs;
            configInterface.Save();
        }

        var retainderColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.RetainerOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-retainer", ref retainderColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.RetainerOption.ColorU = ImGui.ColorConvertFloat4ToU32(retainderColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var showRetainer = configInterface.cfg.ShowRetainer;
        if (ImGui.Checkbox("Retainer", ref showRetainer))
        {
            configInterface.cfg.ShowRetainer = showRetainer;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Shows retainers.");
        }

        DrawSeperator("Objects", UtilInfo.Red);

        var lootColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.TreasureOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-treaure", ref lootColor, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.TreasureOption.ColorU = ImGui.ColorConvertFloat4ToU32(npcColor);
            configInterface.Save();
        }

        ImGui.SameLine();
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

        var eventObjs = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.EventObjOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-eventobjs", ref eventObjs,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.EventObjOption.ColorU = ImGui.ColorConvertFloat4ToU32(eventObjs);
            configInterface.Save();
        }

        ImGui.SameLine();
        var events = configInterface.cfg.ShowEvents;
        if (ImGui.Checkbox("Event Objects", ref events))
        {
            configInterface.cfg.ShowEvents = events;
            configInterface.Save();
        }


        var areaObjs = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.AreaOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-areaobjs", ref areaObjs, ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.AreaOption.ColorU = ImGui.ColorConvertFloat4ToU32(areaObjs);
            configInterface.Save();
        }

        ImGui.SameLine();

        var showAreaObjs = configInterface.cfg.ShowAreaObjects;
        if (ImGui.Checkbox("Area Objects", ref showAreaObjs))
        {
            configInterface.cfg.ShowAreaObjects = showAreaObjs;
            configInterface.Save();
        }

        var aetheriteColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.AetheryteOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-aetherytes", ref aetheriteColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.AetheryteOption.ColorU = ImGui.ColorConvertFloat4ToU32(aetheriteColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var showAetherytes = configInterface.cfg.ShowAetherytes;
        if (ImGui.Checkbox("Aetherytes", ref showAetherytes))
        {
            configInterface.cfg.ShowAetherytes = showAetherytes;
            configInterface.Save();
        }


        var cutsceneObjectColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.CutsceneOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-cutscene", ref cutsceneObjectColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.CutsceneOption.ColorU = ImGui.ColorConvertFloat4ToU32(cutsceneObjectColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var showCutsceneObject = configInterface.cfg.ShowCutscene;
        if (ImGui.Checkbox("Cutscene Objects", ref showCutsceneObject))
        {
            configInterface.cfg.ShowCutscene = showCutsceneObject;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Shows cutscene objects. I have no idea either.");
        }


        DrawSeperator("Misc", UtilInfo.Red);

        var cardStandColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.CardStandOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-cardStand", ref cardStandColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.CardStandOption.ColorU = ImGui.ColorConvertFloat4ToU32(cardStandColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var showCardStand = configInterface.cfg.ShowCardStand;
        if (ImGui.Checkbox("Card Stand (Island Sanctuary Nodes)", ref showCardStand))
        {
            configInterface.cfg.ShowCardStand = showCardStand;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Show card stand. This includes island sanctuary stuff (mostly).");
        }


        var gatheringPointColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.GatheringPointOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-gatheringpointcolor", ref gatheringPointColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.GatheringPointOption.ColorU = ImGui.ColorConvertFloat4ToU32(gatheringPointColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var showGatheringPoint = configInterface.cfg.ShowGatheringPoint;
        if (ImGui.Checkbox("Gathering Point", ref showGatheringPoint))
        {
            configInterface.cfg.ShowGatheringPoint = showGatheringPoint;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Shows Gathering Points");
        }

        var mountPointColor = ImGui.ColorConvertU32ToFloat4(configInterface.cfg.MountOption.ColorU);
        if (ImGui.ColorEdit4($"##visiblitygeneralsettings-color-mountoptioncolor", ref mountPointColor,
                ImGuiColorEditFlags.NoInputs))
        {
            configInterface.cfg.MountOption.ColorU = ImGui.ColorConvertFloat4ToU32(mountPointColor);
            configInterface.Save();
        }

        ImGui.SameLine();
        var showMount = configInterface.cfg.ShowMountType;
        if (ImGui.Checkbox("Mount", ref showMount))
        {
            configInterface.cfg.ShowMountType = showMount;
            configInterface.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Shows mounts. Gets a little cluttered");
        }
        
        ImGui.EndChild();
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