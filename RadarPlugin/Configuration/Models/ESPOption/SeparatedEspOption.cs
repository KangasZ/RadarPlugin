using RadarPlugin.Configuration.Models.ESPOption;

namespace RadarPlugin.Configuration.Models;

public class SeparatedEspOption
{
    public bool Enabled = false;
    public ESPOption.ESPOption EspOption = new(DefaultESPOptions.objectOptDefault);
}
