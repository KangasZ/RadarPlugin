using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Linq;
using System.Threading;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using RadarPlugin;

namespace RadarPlugin;
public class RadarLogic : IDisposable
{
    private DalamudPluginInterface pluginInterface { get; set; }
    private Configuration configInterface { get; set; }
    private DrawPoint myChara { get; set; }

    public RadarLogic(DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        this.pluginInterface = pluginInterface;
        configInterface = configuration;
        PluginLog.Debug($"Radar Loaded");
        myChara = new DrawPoint(Services.ClientState.LocalPlayer);
        this.pluginInterface.UiBuilder.Draw += myChara.DrawUnder;
    }

    public void Dispose()
    {
        PluginLog.Debug($"Radar Unloaded");
    }
}