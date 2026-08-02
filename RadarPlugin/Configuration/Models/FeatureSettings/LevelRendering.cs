using RadarPlugin.Configuration.Models.ESPOption;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class LevelRendering
{
    public bool LevelRenderingEnabled = false;
    public int RelativeLevelsBelow = 20;
    public ESPOption.ESPOption LevelRenderEspOption = new(DefaultESPOptions.mobOptDefault);
}
