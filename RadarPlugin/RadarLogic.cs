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
        this.objectTable = objectTable;
        this.pluginInterface = pluginInterface;
        configInterface = configuration;
        PluginLog.Debug($"Radar Loaded");
        keepRunning = true;
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
            Vector2 onScreenPosition;
            var p = Services.GameGui.WorldToScreen(areaObject.Position, out onScreenPosition);
            if (!p) continue;

            if (areaObject is BattleNpc mob)
            {
                DrawHealthCircle(onScreenPosition, mob, 13f);
            }
            else if (areaObject is GameObject obj)
            {
                var tagText = String.Empty;
                if (UtilInfo.ObjectTrackList.ContainsKey(obj.DataId))
                {
                    tagText = $"{UtilInfo.ObjectTrackList[obj.DataId]}, {obj.DataId}";
                }
                else
                {
                    tagText =
                        $"{areaObject.Name} {areaObject.DataId}";
                }

                var tagTextSize = ImGui.CalcTextSize(tagText);
                //DrawHealthCircle(onScreenPosition, npc, 13f);
                ImGui.GetForegroundDrawList().AddText(
                    new Vector2(onScreenPosition.X - tagTextSize.X / 2f, onScreenPosition.Y + tagTextSize.Y / 2f),
                    UtilInfo.Color(0xFF, 0x7E, 0x00, 0xFF),
                    tagText);
            }
        }
    }

    private void DrawHealthCircle(Vector2 position, BattleNpc npc, float radius)
    {
        // FROM: https://www.unknowncheats.me/forum/direct3d/488372-health-circle-esp-imgui-function.html
        var v1 = (float)npc.CurrentHp / (float)npc.MaxHp;
        var aMax = ((PI * 2.0f));
        var difference = v1 - 1.0f;

        var healthText = ((int)(v1 * 100)).ToString();
        var tagText = $"{npc.Name} {npc.DataId}";

        var healthTextSize = ImGui.CalcTextSize(healthText);
        var tagTextSize = ImGui.CalcTextSize(tagText);
        var colorWhite = UtilInfo.Color(0xff, 0xff, 0xff, 0xff);
        var colorHealth = ImGui.ColorConvertFloat4ToU32(new Vector4(Math.Abs(v1 - difference), v1, v1, 1.0f));
        ImGui.GetForegroundDrawList().PathArcTo(position, radius,
            (-(aMax / 4.0f)) + (aMax / npc.MaxHp) * (npc.MaxHp - npc.CurrentHp), aMax - (aMax / 4.0f), 200 - 1);
        ImGui.GetForegroundDrawList().PathStroke(colorHealth, ImDrawFlags.None, 2.0f);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2((position.X - healthTextSize.X / 2.0f), (position.Y - healthTextSize.Y / 2.0f)),
            colorWhite,
            healthText);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2(position.X - tagTextSize.X / 2f, position.Y + tagTextSize.Y / 2f),
            colorWhite,
            tagText);
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
            if (!obj.IsValid()) continue;
            //if (obj.Name.TextValue.IsNullOrWhitespace()) continue;
            if (obj is BattleChara mob)
            {
                if (obj is BattleNpc npc)
                {
                    if (npc.BattleNpcKind != BattleNpcSubKind.Enemy)
                        continue;
                }

                if (mob.CurrentHp <= 0) continue;
                if (!configInterface.ShowPlayers && obj.SubKind == 4) continue;
                if (configInterface.UseObjectHideList)
                {
                    if (UtilInfo.BossFixList.ContainsKey(mob.NameId) &&
                        mob.DataId != UtilInfo.BossFixList[mob.NameId]) continue;
                    if (UtilInfo.IgnoreList.Contains(mob.NameId)) continue;
                }

                nearbyMobs.Add(obj);
            }
            else
            {
                if (!configInterface.ObjectShow) continue;
                if (configInterface.UseObjectHideList)
                {
                    if (UtilInfo.ObjectTrackList.ContainsKey(obj.DataId) ||
                        UtilInfo.ObjectStringList.Contains(obj.Name.TextValue))
                    {
                        nearbyMobs.Add(obj);
                    }

                    continue;
                }

                nearbyMobs.Add(obj);
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