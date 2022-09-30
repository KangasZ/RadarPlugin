using Dalamud.Game.ClientState.Objects;
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
    private ObjectTable objectTable { get; set; }
    
    public RadarPlugin(
        DalamudPluginInterface pluginInterface,
        CommandManager commandManager,
        ObjectTable objectTable)
    {
        pluginInterface.Create<Services>(); // Todo: Remove this
        this.objectTable = objectTable;
        configuration = new Configuration(pluginInterface);
        pluginUi = new PluginUi(pluginInterface, configuration);
        pluginCommands = new PluginCommands(commandManager, pluginUi);
        radarLogic = new RadarLogic(pluginInterface, configuration, this.objectTable);
    }

    public void Dispose()
    {
        pluginCommands.Dispose();
        radarLogic.Dispose();
    }
}