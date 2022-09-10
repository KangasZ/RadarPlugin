using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace RadarPlugin;

public sealed class RadarPlugin : IDalamudPlugin
{
    public string Name => "Radar Plugin";
    private RadarLogic radarLogic { get; set; }
    private PluginCommands pluginCommands { get; set; }
    private Configuration configuration { get; set; }
    private PluginUi pluginUi { get; set; }
    
    public RadarPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        pluginInterface.Create<Services>(); // Todo: Remove this

        configuration = new Configuration(pluginInterface);
        pluginUi = new PluginUi(pluginInterface, configuration);
        pluginCommands = new PluginCommands(commandManager, pluginUi);
        radarLogic = new RadarLogic(pluginInterface, configuration);
    }

    public void Dispose()
    {
        pluginCommands.Dispose();
        radarLogic.Dispose();
    }
}