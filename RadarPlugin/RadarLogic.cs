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
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using RadarPlugin;

namespace RadarPlugin;
public class RadarLogic : IDisposable
{
    private const float PI = 3.14159265359f;
    private DalamudPluginInterface pluginInterface { get; set; }
    private Configuration configInterface { get; set; }
    private DrawPoint myChara { get; set; }
    private Task backgroundLoop { get; set; }
    private bool keepRunning { get; set; }
    private ObjectTable objectTable { get; set; }
    private List<BattleNpc> currentMobs { get; set; }
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

        currentMobs = new List<BattleNpc>();
    }

    private void DrawRadar()
    {
        if (!configInterface.Enabled) return;
        if (refreshing) return;
        foreach (var npc in currentMobs)
        {
            if (!npc.IsValid()) continue;
            if (npc.CurrentHp <= 0) continue;
            if (npc.CurrentMp <= 0) continue;
            Vector2 onScreenPosition;
            var p = Services.GameGui.WorldToScreen(npc.Position, out onScreenPosition);
            if (!p) continue;
            uint color =
                ImGui.ColorConvertFloat4ToU32(Info.HuntRecolors.ContainsKey(npc.NameId)
                    ? Info.HuntRecolors[npc.NameId]
                    : new Vector4(1,0,0,1));
            
            //PluginLog.Debug($"Creating vector for character: {ObjectDraw.Name} at {X}, {Y}, {Z} : 2D Vector at {vector2.X}, {vector2.Y}");
            /*ImGui.GetForegroundDrawList().AddCircleFilled(vector2, 5f, color, 8);*/
            var text = $"{npc.Name}-{npc.NameId}";
            var textSize = ImGui.CalcTextSize(text);
            DrawHealthCircle(onScreenPosition, npc, 13f);
            
        }
    }
    
    private void DrawHealthCircle(Vector2 position, BattleNpc npc, float radius)
    {
        // FROM: https://www.unknowncheats.me/forum/direct3d/488372-health-circle-esp-imgui-function.html
        var v1 = (float)npc.CurrentHp / (float)npc.MaxHp;
        var aMax = ((PI * 2.0f));
        var difference = v1 - 1.0f;

        var healthText = ((int)(v1*100)).ToString();
        var tagText = $"{npc.Name}, {npc.NameId}";
        
        var healthTextSize = ImGui.CalcTextSize(healthText);
        var tagTextSize = ImGui.CalcTextSize(tagText);
        var colorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1.0f));
        var colorHealth = ImGui.ColorConvertFloat4ToU32(new Vector4(Math.Abs(v1 - difference), v1, v1, 1.0f));
        ImGui.GetForegroundDrawList().PathArcTo(position, radius, (-(aMax / 4.0f)) + (aMax / npc.MaxHp) * (npc.MaxHp - npc.CurrentHp), aMax - (aMax / 4.0f), 200 - 1);
        ImGui.GetForegroundDrawList().PathStroke(colorHealth, ImDrawFlags.None, 2.0f);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2((position.X - healthTextSize.X / 2.0f), (position.Y - healthTextSize.Y / 2.0f)),
            colorWhite,
            healthText);
        ImGui.GetForegroundDrawList().
            AddText(
                new Vector2(position.X - tagTextSize.X/2f, position.Y + tagTextSize.Y/2f), 
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
            Thread.Sleep(2000);
        }
    }
    
    private void UpdateMobInfo()
    {
        var nearbyMobs = new List<BattleNpc>();

        foreach (var obj in objectTable)
        {
            if (obj is not BattleNpc mob) continue;
            if (mob.BattleNpcKind != BattleNpcSubKind.Enemy) continue;
            if (mob.CurrentMp <= 0) continue;
            nearbyMobs.Add(mob);
        }

        refreshing = true; // TODO change off refreshing
        currentMobs.Clear();
        currentMobs.AddRange(nearbyMobs);
        refreshing = false;

    }

    public void Dispose()
    {
        keepRunning = false;
        while (!backgroundLoop.IsCompleted) ;
        PluginLog.Debug($"Radar Unloaded");
    }
}