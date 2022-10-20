using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using RadarPlugin;

namespace RadarPlugin;

public class RadarLogic : IDisposable
{
    private const float PI = 3.14159265359f;
    private DalamudPluginInterface pluginInterface { get; set; }
    private Configuration configInterface { get; set; }
    private Task backgroundLoop { get; set; }
    private bool keepRunning { get; set; }
    private ObjectTable objectTable { get; set; }
    private List<GameObject> areaObjects { get; set; }
    private bool refreshing { get; set; }

    public RadarLogic(DalamudPluginInterface pluginInterface, Configuration configuration, ObjectTable objectTable)
    {
        // Creates Dependencies
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        this.configInterface = configuration;

        // Loads plugin
        PluginLog.Debug($"Radar Loaded");
        keepRunning = true;
        // TODO: In the future adjust this
        this.pluginInterface.UiBuilder.Draw += DrawRadar;
        backgroundLoop = Task.Run(BackgroundLoop);

        areaObjects = new List<GameObject>();
    }

    private void DrawRadar()
    {
        if (!configInterface.Enabled) return;
        if (refreshing) return;
        foreach (var areaObject in areaObjects)
        {
            var p = Services.GameGui.WorldToScreen(areaObject.Position, out var onScreenPosition);
            if (!p) continue;

            var tagText = GetText(areaObject);

            switch (areaObject)
            {
                // Mobs
                case BattleNpc mob:
                    var colorWhite = UtilInfo.Color(0xff, 0xff, 0xff, 0xff);
                    DrawEsp(onScreenPosition, mob, tagText, colorWhite, drawHealthCircle: true);
                    break;
                // Players
                case BattleChara chara:
                {
                    var color = UtilInfo.Color(0x00, 0x99, 0x99, 0xff);
                    DrawEsp(onScreenPosition, chara, tagText, color);
                    break;
                }
                // Objects
                default:
                {
                    if (UtilInfo.RenameList.ContainsKey(areaObject.DataId))
                    {
                        tagText = UtilInfo.RenameList[areaObject.DataId];
                    }

                    var tagTextSize = ImGui.CalcTextSize(tagText);

                    ImGui.GetForegroundDrawList().AddText(
                        new Vector2(onScreenPosition.X - tagTextSize.X / 2f, onScreenPosition.Y + tagTextSize.Y / 2f),
                        UtilInfo.Color(0xFF, 0x7E, 0x00, 0xFF), //  #FF7E00
                        tagText);
                    break;
                }
            }
        }
    }

    private void DrawEsp(Vector2 position, Character npc, string mobText, uint color, bool drawHealthCircle = false)
    {
        if (drawHealthCircle) // TODO: Make config option
        {
            DrawHealthCircle(position, npc.MaxHp, npc.CurrentHp);
        }

        var tagTextSize = ImGui.CalcTextSize(mobText);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2(position.X - tagTextSize.X / 2f, position.Y + tagTextSize.Y / 2f),
            color,
            mobText);
    }

    private void DrawHealthCircle(Vector2 position, uint maxHp, uint currHp, bool includeText = true)
    {
        const float radius = 13f;
        var v1 = (float)currHp / (float)maxHp;
        var aMax = PI * 2.0f;
        var difference = v1 - 1.0f;

        var healthText = ((int)(v1 * 100)).ToString();
        var colorWhite = UtilInfo.Color(0xff, 0xff, 0xff, 0xff);
        var colorHealth = ImGui.ColorConvertFloat4ToU32(new Vector4(Math.Abs(v1 - difference), v1, v1, 1.0f));
        ImGui.GetForegroundDrawList().PathArcTo(position, radius,
            (-(aMax / 4.0f)) + (aMax / maxHp) * (maxHp - currHp), aMax - (aMax / 4.0f), 200 - 1);
        ImGui.GetForegroundDrawList().PathStroke(colorHealth, ImDrawFlags.None, 2.0f);
        if (!includeText) return;
        var healthTextSize = ImGui.CalcTextSize(healthText);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2((position.X - healthTextSize.X / 2.0f), (position.Y - healthTextSize.Y / 2.0f)),
            colorWhite,
            healthText);
    }

    private string GetText(GameObject obj)
    {
        if (configInterface.DebugMode)
        {
            return $"{obj.Name}, {obj.DataId}";
        }

        return $"{obj.Name}";
    }

    private void BackgroundLoop()
    {
        while (keepRunning)
        {
            if (configInterface.Enabled)
            {
                UpdateMobInfo();
                PluginLog.Debug("Refreshed Mob Info!");
            }

            Thread.Sleep(1000);
        }
    }

    private void UpdateMobInfo()
    {
        var nearbyMobs = new List<GameObject>();

        foreach (var obj in objectTable)
        {
            if (configInterface.DebugMode)
            {
                nearbyMobs.Add(obj);
                continue;
            }

            if (!obj.IsValid()) continue;
            switch (obj)
            {
                // Mobs
                case BattleNpc mob:
                    if (String.IsNullOrWhiteSpace(mob.Name.TextValue)) continue;
                    if (mob.BattleNpcKind != BattleNpcSubKind.Enemy) continue;
                    if (mob.CurrentHp <= 0) continue;
                    if (!configInterface.ShowPlayers && obj.SubKind == 4) continue;
                    if (UtilInfo.DataIdIgnoreList.Contains(mob.DataId) ||
                        configInterface.DataIdIgnoreList.Contains(mob.DataId)) continue;
                    nearbyMobs.Add(obj);
                    continue;
                // Players
                case BattleChara chara:
                    if (!configInterface.ShowPlayers) continue;
                    if (chara.CurrentHp <= 0) continue;
                    nearbyMobs.Add(obj);
                    continue;
                // Objects
                default:
                    if (!configInterface.ObjectShow) continue;
                    if (UtilInfo.RenameList.ContainsKey(obj.DataId) ||
                        UtilInfo.ObjectStringList.Contains(obj.Name.TextValue))
                    {
                        nearbyMobs.Add(obj);
                    }
                    continue;
            }
        }

        refreshing = true; // TODO change off refreshing
        areaObjects.Clear();
        areaObjects.AddRange(nearbyMobs);
        refreshing = false;
    }

    public void Dispose()
    {
        keepRunning = false;
        while (!backgroundLoop.IsCompleted) ;
        PluginLog.Debug($"Radar Unloaded");
    }
}