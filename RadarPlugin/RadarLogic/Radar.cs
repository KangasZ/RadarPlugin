using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using RadarPlugin.Constants;
using RadarPlugin.Enums;
using RadarPlugin.RadarLogic.Modules;
using RadarPlugin.UI;
using GameObject = Dalamud.Game.ClientState.Objects.Types.GameObject;

namespace RadarPlugin.RadarLogic;

public class Radar : IDisposable
{
    private readonly DalamudPluginInterface pluginInterface;
    private Configuration.Configuration configInterface;
    private readonly ICondition conditionInterface;
    private readonly IObjectTable objectTable;
    private readonly IClientState clientState;
    private readonly IGameGui gameGui;
    private readonly IPluginLog pluginLog;

    private GameFontHandle? gameFont;
    private ImFontPtr? dalamudFont;
    private bool fontBuilt = false;

    private RadarModules radarModules;
    
    public Radar(DalamudPluginInterface pluginInterface, Configuration.Configuration configuration,
        IObjectTable objectTable,
        ICondition condition, IClientState clientState, IGameGui gameGui,
        IPluginLog pluginLog, RadarModules radarModules)
    {
        // Creates Dependencies
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;
        this.conditionInterface = condition;
        this.gameGui = gameGui;
        this.radarModules = radarModules;
        this.pluginLog = pluginLog;
        // Loads plugin
        this.pluginLog.Debug("Radar Loaded");

        this.clientState = clientState;

        this.pluginInterface.UiBuilder.Draw += OnUiTick;
        this.pluginInterface.UiBuilder.BuildFonts += BuildFont;
    }

    private void BuildFont()
    {
        fontBuilt = false;
        var fontFile = Path.Combine(pluginInterface.DalamudAssetDirectory.FullName, "UIRes",
            "NotoSansCJKjp-Medium.otf");
        if (File.Exists(fontFile))
        {
            try
            {
                dalamudFont = ImGui.GetIO().Fonts
                    .AddFontFromFileTTF(fontFile, configInterface.cfg.FontSettings.FontSize);
                fontBuilt = true;
                this.pluginLog.Debug("Custom dalamud font loaded sucesffully");
            }
            catch (Exception ex)
            {
                this.pluginLog.Error(ex, "Font failed to load!");
            }
        }
        else
        {
            this.pluginLog.Error("Font does not exist! Please fix dev.");
        }
    }

    private void OnUiTick()
    {
        if (!configInterface.cfg.Enabled) return;

        ImFontPtr fontPtr = LoadFont();
        using var font = ImRaii.PushFont(fontPtr);
        if (objectTable.Length == 0) return;
        if (CheckDraw()) return;
        RadarOnTick();
    }


    private ImFontPtr LoadFont()
    {
        ImFontPtr fontPtr;
        if (configInterface.cfg.FontSettings.UseCustomFont)
        {
            if (configInterface.cfg.FontSettings.UseAxisFont)
            {
                if (this.gameFont != null && this.gameFont.Available)
                {
                    var tempPointer = gameFont.ImFont;
                    if (tempPointer.IsLoaded() &&
                        Math.Abs(tempPointer.FontSize - configInterface.cfg.FontSettings.FontSize) < 0.01)
                    {
                        return this.gameFont.ImFont;
                    }
                }
                
                
                gameFont = pluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis,
                    configInterface.cfg.FontSettings.FontSize));
                return gameFont.ImFont;
            }

            if (dalamudFont.HasValue && this.fontBuilt && dalamudFont.Value.IsLoaded() &&
                Math.Abs(dalamudFont.Value.FontSize - configInterface.cfg.FontSettings.FontSize) < 0.01)
            {
                return dalamudFont.Value;
            }

            pluginInterface.UiBuilder.RebuildFonts();
            return this.fontBuilt ? dalamudFont.Value : ImGui.GetFont();
        }

        fontPtr = ImGui.GetFont();

        return fontPtr;
    }

    private void RadarOnTick()
    {
        // Setup Drawlist
        var bgDl = configInterface.cfg.UseBackgroundDrawList;
        var requiresSave = false;
        ImDrawListPtr drawListPtr;
        if (bgDl)
        {
            drawListPtr = ImGui.GetBackgroundDrawList();
        }
        else
        {
            ImGui.Begin("RadarPluginOverlay",
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoMouseInputs |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoNav |
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoFocusOnAppearing);
            var mainViewPort = ImGui.GetMainViewport();

            ImGui.SetWindowPos(mainViewPort.Pos);
            ImGui.SetWindowSize(mainViewPort.Size);
            drawListPtr = ImGui.GetWindowDrawList();
        }

        // Figure out object table
        IEnumerable<GameObject> objectTableRef;
        if (configInterface.cfg.DebugMode)
        {
            objectTableRef = objectTable;
        }
        else
        {
            objectTableRef = objectTable.Where(ShouldRenderObject);
            if (configInterface.cfg.UseMaxDistance && clientState.LocalPlayer != null)
            {
                objectTableRef = objectTableRef.Where(objectToFind =>
                    radarModules.distanceModule.GetDistanceFromPlayer(clientState.LocalPlayer, objectToFind) <
                    configInterface.cfg.MaxDistance);
            }
        }

        foreach (var areaObject in objectTableRef)
        {
            var espOption = radarModules.radarConfigurationModule.GetParamsWithOverride(areaObject);
            // Temporary script that updates the new option override with current settings if it hasn't been migrated
            if (configInterface.cfg.ColorOverride.TryGetValue(areaObject.DataId, out var optionOverride) &&
                !configInterface.cfg.OptionOverride.ContainsKey(areaObject.DataId))
            {
                var newcolor = new Configuration.Configuration.ESPOptionMobBased(espOption);
                newcolor.ColorU = optionOverride;
                configInterface.cfg.ColorOverride.Remove(areaObject.DataId);
                var name = areaObject.Name.TextValue ?? "Unknown";
                pluginLog.Information("Migrated {Name} to new override system", name);
                configInterface.CustomizeMob(areaObject, true, newcolor);
                espOption = newcolor;
            }

            if (!espOption.Enabled && !configInterface.cfg.DebugMode) continue;
            DrawEsp(drawListPtr, areaObject, espOption.ColorU, espOption);
        }

        if (!bgDl)
        {
            ImGui.End();
        }
    }

    /**
     * Returns true if you should not draww
     */
    private bool CheckDraw()
    {
        return conditionInterface[ConditionFlag.LoggingOut] || conditionInterface[ConditionFlag.BetweenAreas] ||
               conditionInterface[ConditionFlag.BetweenAreas51] ||
               clientState.LocalContentId == 0 || clientState.LocalPlayer == null;
    }

    private void DrawEsp(ImDrawListPtr drawListPtr, GameObject gameObject, uint? overrideColor,
        Configuration.Configuration.ESPOption espOption)
    {
        var color = overrideColor ?? espOption.ColorU;
        var visibleOnScreen = gameGui.WorldToScreen(gameObject.Position, out var onScreenPosition);
        if (visibleOnScreen)
        {
            var dotSize = espOption.DotSizeOverride ? espOption.DotSize : configInterface.cfg.DotSize;
            switch (espOption.DisplayType)
            {
                case DisplayTypes.DotOnly:
                    DrawRadarHelper.DrawDot(drawListPtr, onScreenPosition, dotSize, color);
                    break;
                case DisplayTypes.NameOnly:
                    var nameOnlyText = radarModules.radarConfigurationModule.GetText(gameObject, espOption);
                    DrawRadarHelper.DrawTextCenteredUnder(drawListPtr, onScreenPosition, nameOnlyText, color,
                        espOption);
                    break;
                case DisplayTypes.DotAndName:
                    DrawRadarHelper.DrawDot(drawListPtr, onScreenPosition, dotSize, color);
                    var dotAndNameText = radarModules.radarConfigurationModule.GetText(gameObject, espOption);
                    DrawRadarHelper.DrawTextCenteredUnder(drawListPtr, onScreenPosition, dotAndNameText, color,
                        espOption);
                    break;
                case DisplayTypes.HealthBarOnly:
                    DrawRadarHelper.DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthBarAndValue:
                    DrawRadarHelper.DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    DrawRadarHelper.DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthBarAndName:
                    DrawRadarHelper.DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    var healthBarAndName = radarModules.radarConfigurationModule.GetText(gameObject, espOption);
                    DrawRadarHelper.DrawTextCenteredUnder(drawListPtr, onScreenPosition, healthBarAndName, color,
                        espOption);
                    break;
                case DisplayTypes.HealthBarAndValueAndName:
                    DrawRadarHelper.DrawHealthCircle(drawListPtr, onScreenPosition, gameObject, color);
                    var healthBarAndValueAndName = radarModules.radarConfigurationModule.GetText(gameObject, espOption);
                    DrawRadarHelper.DrawTextCenteredUnder(drawListPtr, onScreenPosition, healthBarAndValueAndName,
                        color, espOption);
                    DrawRadarHelper.DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthValueOnly:
                    DrawRadarHelper.DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    break;
                case DisplayTypes.HealthValueAndName:
                    DrawRadarHelper.DrawHealthValue(drawListPtr, onScreenPosition, gameObject, color);
                    var healthValueAndName = radarModules.radarConfigurationModule.GetText(gameObject, espOption);
                    DrawRadarHelper.DrawTextCenteredUnder(drawListPtr, onScreenPosition, healthValueAndName, color,
                        espOption);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (configInterface.cfg.ShowOffScreen)
        {
            UiHelpers.GetBorderClampedVector2(onScreenPosition,
                new Vector2(configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge,
                    configInterface.cfg.OffScreenObjectsOptions.DistanceFromEdge), out var clampedPos);
            var mainViewport3 = ImGui.GetMainViewport();
            var center2 = mainViewport3.GetCenter();
            var rotation = clampedPos - center2;
            drawListPtr.DrawArrow(clampedPos, configInterface.cfg.OffScreenObjectsOptions.Size, color, rotation,
                configInterface.cfg.OffScreenObjectsOptions.Thickness);
        }

        switch (gameObject)
        {
            case BattleNpc npc2:
            {
                if (configInterface.cfg.HitboxOptions.HitboxEnabled)
                {
                    uint colorResolved;

                    if (configInterface.cfg.HitboxOptions.OverrideMobColor)
                    {
                        colorResolved = configInterface.cfg.HitboxOptions.HitboxColor;
                    }
                    else
                    {
                        colorResolved = color;
                    }

                    //opacity = configInterface.cfg.AggroRadiusOptions.CircleOpacity;
                    DrawHitbox(drawListPtr, gameObject.Position, gameObject.HitboxRadius,
                        colorResolved, configInterface.cfg.HitboxOptions.Thickness);

                    if (configInterface.cfg.HitboxOptions.DrawInsideCircle)
                    {
                        uint insideColorResolved;
                        if (configInterface.cfg.HitboxOptions.UseDifferentInsideCircleColor)
                        {
                            insideColorResolved = configInterface.cfg.HitboxOptions.InsideCircleColor;
                        }
                        else
                        {
                            insideColorResolved = colorResolved & configInterface.cfg.HitboxOptions.InsideCircleOpacity;
                        }

                        DrawRadarHelper.DrawConeAtCenterPointFromRotation(drawListPtr, gameObject.Position,
                            gameObject.Rotation,
                            MathF.PI * 2, gameObject.HitboxRadius,
                            insideColorResolved, 100, gameGui);
                    }
                }

                if (configInterface.cfg.AggroRadiusOptions.ShowAggroCircle)
                {
                    // Aggro radius max distance check
                    if (configInterface.cfg.AggroRadiusOptions.MaxDistanceCapBool && clientState.LocalPlayer != null &&
                        radarModules.distanceModule.GetDistanceFromPlayer(clientState.LocalPlayer, npc2) >
                        configInterface.cfg.AggroRadiusOptions.MaxDistance)
                    {
                        return;
                    }

                    if (!configInterface.cfg.AggroRadiusOptions.ShowAggroCircleInCombat &&
                        (npc2.StatusFlags & StatusFlags.InCombat) != 0) return;
                    if (npc2.BattleNpcKind != BattleNpcSubKind.Enemy) return;
                    float aggroRadius = 10;
                    if (MobConstants.AggroDistance.TryGetValue(gameObject.DataId, out var range))
                    {
                        aggroRadius = range;
                    }

                    DrawAggroRadius(drawListPtr, gameObject.Position, aggroRadius + gameObject.HitboxRadius,
                        gameObject.Rotation,
                        uint.MaxValue, npc2);
                }

                break;
            }
            case PlayerCharacter pc when espOption.ShowMp:
                DrawMp(drawListPtr, onScreenPosition, pc, color);
                break;
        }
    }

    //todo better
    private void DrawMp(ImDrawListPtr imDrawListPtr, Vector2 position, PlayerCharacter gameObject,
        uint playerOptColor)
    {
        var mpText = gameObject.CurrentMp.ToString();
        var mptextSize = ImGui.CalcTextSize(mpText);
        imDrawListPtr.AddText(
            new Vector2((position.X - mptextSize.X / 2.0f), (position.Y + mptextSize.Y * 1.5f)),
            playerOptColor,
            mpText);
    }

    private void DrawHitbox(ImDrawListPtr drawListPtr, Vector3 gameObjectPosition, float gameObjectHitboxRadius,
        uint color, float thickness)
    {
        DrawRadarHelper.DrawArcAtCenterPointFromRotations(drawListPtr, gameObjectPosition, 0, 2 * MathF.PI,
            gameObjectHitboxRadius,
            color, thickness, 400, gameGui);
    }

    private void DrawAggroRadius(ImDrawListPtr imDrawListPtr, Vector3 position, float radius, float rotation,
        uint objectOptionColor, BattleNpc battleNpc)
    {
        var opacity = configInterface.cfg.AggroRadiusOptions.CircleOpacity;
        rotation += MathF.PI / 4;
        var numSegments = 100;

        var thickness = 2f;

        //todo: handle CONE
        //todo: shove opacity into color 
        var aggroType = radarModules.aggroTypeModule.GetAggroType(battleNpc.NameId);
        switch (aggroType)
        {
            case AggroType.Proximity:
                var proximityColor = configInterface.cfg.AggroRadiusOptions.FrontColor & opacity;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation, MathF.PI * 2,
                    radius,
                    proximityColor,
                    thickness, numSegments, gameGui);
                break;
            case AggroType.Sound:
                var soundColor = configInterface.cfg.AggroRadiusOptions.RearColor & opacity;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation, MathF.PI * 2,
                    radius,
                    soundColor, thickness, numSegments, gameGui);
                break;
            case AggroType.Sight:
            default:
                var frontColor = configInterface.cfg.AggroRadiusOptions.FrontColor & opacity;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation, MathF.PI / 2,
                    radius, frontColor,
                    thickness, numSegments, gameGui);
                var rightColor = configInterface.cfg.AggroRadiusOptions.RightSideColor & opacity;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation + MathF.PI / 2,
                    MathF.PI / 2,
                    radius,
                    rightColor, thickness, numSegments, gameGui);
                var backColor = configInterface.cfg.AggroRadiusOptions.RearColor & opacity;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation + MathF.PI,
                    MathF.PI / 2, radius,
                    backColor, thickness, numSegments, gameGui);
                var leftColor = configInterface.cfg.AggroRadiusOptions.LeftSideColor & opacity;
                DrawRadarHelper.DrawArcAtCenterPointFromRotations(imDrawListPtr, position, rotation + (MathF.PI * 1.5f),
                    MathF.PI / 2,
                    radius,
                    leftColor, thickness, numSegments, gameGui);
                var coneColor = configInterface.cfg.AggroRadiusOptions.FrontConeColor &
                                configInterface.cfg.AggroRadiusOptions.FrontConeOpacity;
                DrawRadarHelper.DrawConeAtCenterPointFromRotation(imDrawListPtr, position, rotation, MathF.PI / 2,
                    radius, coneColor,
                    50, gameGui);
                break;
        }

        imDrawListPtr.PathClear();
    }

    private unsafe bool ShouldRenderObject(GameObject obj)
    {
        // Objest valid check
        if (!obj.IsValid()) return false;
        //if (obj.DataId == GameObject.InvalidGameObjectId) return false;

        // Object within ignore lists
        if (MobConstants.DataIdIgnoreList.Contains(obj.DataId)) return false;

        // Object visible & config check
        var clientstructobj = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)(void*)obj.Address;
        if (configInterface.cfg.ShowOnlyVisible &&
            (clientstructobj->RenderFlags != 0)) // || !clientstructobj->GetIsTargetable()))
        {
            // If override is not enabled, return false, otherwise check if the object kind is a player, and if not, return false still. 
            if (!configInterface.cfg.OverrideShowInvisiblePlayerCharacters) return false;
            if (obj.ObjectKind != ObjectKind.Player) return false;
        }

        var locationType = radarModules.zoneTypeModule.GetLocationType();
        
        // Overworld Enable Check
        if (!configInterface.cfg.ShowOverworldObjects && locationType == LocationKind.Overworld)
        {
            return false;
        }
        
        // Eureka DD STQ
        if (configInterface.cfg.ShowBaDdObjects && radarModules.zoneTypeModule.GetLocationType() == LocationKind.DeepDungeon)
        {
            // UtilInfo.RenameList.ContainsKey(obj.DataId) || UtilInfo.DeepDungeonMobTypesMap.ContainsKey(obj.DataId)))
            if (MobConstants.DeepDungeonMobTypesMap.ContainsKey(obj.DataId)) return true;
            if (string.IsNullOrWhiteSpace(obj.Name.TextValue) && !configInterface.cfg.ShowNameless) return false;
            if (obj.ObjectKind != ObjectKind.BattleNpc || obj is not BattleNpc
                {
                    BattleNpcKind: BattleNpcSubKind.Enemy
                } mob) return true;
            if (!configInterface.cfg.DeepDungeonOptions.DefaultEnemyOption.Enabled) return false;
            return !mob.IsDead;
        }
        
        if (obj is BattleChara mobNpc)
        {
            //if (!clientstructobj->GetIsTargetable()) continue;
            //if (String.IsNullOrWhiteSpace(mob.Name.TextValue)) continue;
            if (string.IsNullOrWhiteSpace(obj.Name.TextValue) && !configInterface.cfg.ShowNameless) return false;
            if (mobNpc.IsDead) return false;
        }

        return true;
    }
    
    
    #region CLEANUP REGION

    public void Dispose()
    {
        pluginInterface.UiBuilder.Draw -= OnUiTick;
        this.pluginInterface.UiBuilder.BuildFonts -= BuildFont;
        pluginLog.Information("Radar Unloaded");
    }

    #endregion
}