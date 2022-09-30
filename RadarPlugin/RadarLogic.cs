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
            Vector2 vector2;
            var p = Services.GameGui.WorldToScreen(npc.Position, out vector2);
            if (!p) continue;
            uint color =
                ImGui.ColorConvertFloat4ToU32(Info.HuntRecolors.ContainsKey(npc.NameId)
                    ? Info.HuntRecolors[npc.NameId]
                    : new Vector4(1,0,0,1));
            
            //PluginLog.Debug($"Creating vector for character: {ObjectDraw.Name} at {X}, {Y}, {Z} : 2D Vector at {vector2.X}, {vector2.Y}");
            ImGui.GetForegroundDrawList().AddCircleFilled(vector2, 5f, color, 8);
            var text = $"{npc.Name}-{npc.NameId}";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.GetForegroundDrawList().
                AddText(
                    new Vector2((vector2.X - textSize.X/2f), (vector2.Y + textSize.X/2f)), 
                    color, 
                    $"{npc.Name} {npc.NameId}");
        }
    }
    
    private void DrawHealthCircle(Vector2 position, int health, int max_health, float radius)
    {
        // FROM: https://www.unknowncheats.me/forum/direct3d/488372-health-circle-esp-imgui-function.html
        float PI = 3.14159265359f;
        string health_text = max_health.ToString();
        float a_max = ((PI * 2.0f));
        float v1 = (float)health / (float)max_health;
        float difference = v1 - 1.0f;
        Vector4 colorVector = new Vector4(Math.Abs(v1 - difference), v1, v1, 1.0f);
        ImGui.GetForegroundDrawList().PathArcTo(position, radius, (-(a_max / 4.0f)) + (a_max / max_health) * (max_health - health), a_max - (a_max / 4.0f), 200 - 1);
        ImGui.GetForegroundDrawList().PathStroke(ImGui.ColorConvertFloat4ToU32(colorVector), ImDrawFlags.None, 2.0f);
        ImGui.GetForegroundDrawList().AddText(
            new Vector2((position.X - ImGui.CalcTextSize(health_text).X / 2.0f), (position.Y - ImGui.CalcTextSize(health_text).Y / 2.0f)),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1,1,1,1.0f)),
            health_text);
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