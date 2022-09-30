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
            Vector2 vector2;
            var p = Services.GameGui.WorldToScreen(npc.Position, out vector2);
            if (!p) continue;
            uint color =
                ImportantHunts.Hunts.ContainsKey(npc.NameId)
                    ? ImportantHunts.Hunts[npc.NameId]
                    : 0xFF0000FF;

            //PluginLog.Debug($"Creating vector for character: {ObjectDraw.Name} at {X}, {Y}, {Z} : 2D Vector at {vector2.X}, {vector2.Y}");
            ImGui.GetForegroundDrawList().AddCircleFilled(vector2, 5f, color, 8);
            ImGui.GetForegroundDrawList().
                AddText(
                    new Vector2((vector2.X - 30), (vector2.Y + 20)), 
                    color, 
                    $"{npc.Name} {npc.NameId}");

        }
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