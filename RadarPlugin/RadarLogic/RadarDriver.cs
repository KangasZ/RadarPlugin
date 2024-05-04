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

public class RadarDriver : IDisposable
{
    private readonly DalamudPluginInterface pluginInterface;
    private Configuration.Configuration configInterface;
    private readonly ICondition conditionInterface;
    private readonly IObjectTable objectTable;
    private readonly IClientState clientState;
    private readonly IGameGui gameGui;
    private readonly IPluginLog pluginLog;
    private readonly Radar3D radar3D;
    private readonly Radar2D radar2D;
    private GameFontHandle? gameFont;
    private ImFontPtr? dalamudFont;
    private bool fontBuilt = false;

    private RadarModules radarModules;

    
    
    
    public RadarDriver(DalamudPluginInterface pluginInterface, Configuration.Configuration configuration,
        IObjectTable objectTable,
        ICondition condition, IClientState clientState, IGameGui gameGui,
        IPluginLog pluginLog, RadarModules radarModules, IGameInteropProvider gameInteropProvider)
    {
        // Creates Dependencies
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;
        this.conditionInterface = condition;
        this.gameGui = gameGui;
        this.radarModules = radarModules;
        this.pluginLog = pluginLog;
        this.radar3D = new Radar3D(configuration, clientState, gameGui, pluginLog, radarModules);
        this.radar2D = new Radar2D(this.pluginInterface, configuration, clientState, this.pluginLog, radarModules);
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
        // Figure out object table
        var objectTableRef = FilterObjectTable(objectTable);
        var objectsWithMobOptions = objectTableRef.Select(areaObject =>
        {
            var espOption = radarModules.radarConfigurationModule.TryGetOverridenParams(areaObject, out var _);
            return (areaObject, espOption);
        }).ToArray();
        radar3D.Radar3DOnTick(objectsWithMobOptions);
        radar2D.Radar2DOnTick(objectsWithMobOptions);
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

    private IEnumerable<GameObject> FilterObjectTable(IObjectTable objectTable)
    {
        IEnumerable<GameObject> objectTableRef;
        if (configInterface.cfg.DebugMode)
        {
            objectTableRef = objectTable;
        }
        else
        {
            objectTableRef = objectTable.Where(ShouldRenderObject);
            objectTableRef = objectTableRef.Where(PassDistanceCheck);
        }

        return objectTableRef;
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


    private bool PassDistanceCheck(GameObject obj)
    {
        if (configInterface.cfg.UseMaxDistance && clientState.LocalPlayer != null)
        {
            return radarModules.distanceModule.GetDistanceFromPlayer(clientState.LocalPlayer, obj) <
                   configInterface.cfg.MaxDistance;
        }

        return true;
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
        if (configInterface.cfg.ShowBaDdObjects &&
            radarModules.zoneTypeModule.GetLocationType() == LocationKind.DeepDungeon)
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