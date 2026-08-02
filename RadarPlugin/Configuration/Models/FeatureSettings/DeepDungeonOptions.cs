using RadarPlugin.Configuration.Models.ESPOption;
using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class DeepDungeonOptions
{
    public ESPOption.ESPOption SpecialUndeadOption { get; set; } =
        new(DefaultESPOptions.mobOptDefault) { ColorU = Color.Yellow };
    public ESPOption.ESPOption AuspiceOption { get; set; } =
        new(DefaultESPOptions.mobOptDefault) { ColorU = Color.Green };
    public ESPOption.ESPOption EasyMobOption { get; set; } =
        new(DefaultESPOptions.mobOptDefault) { ColorU = Color.LightBlue };
    public ESPOption.ESPOption TrapOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Orange };
    public ESPOption.ESPOption ReturnOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Blue };
    public ESPOption.ESPOption PassageOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Blue };
    public ESPOption.ESPOption GoldChestOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Gold };
    public ESPOption.ESPOption SilverChestOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Silver };
    public ESPOption.ESPOption BronzeChestOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Bronze };
    public ESPOption.ESPOption MimicOption { get; set; } =
        new(DefaultESPOptions.mobOptDefault) { ColorU = Color.Red };

    public ESPOption.ESPOption AccursedHoardOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = ConfigConstants.Turquoise };

    public ESPOption.ESPOption DefaultEnemyOption { get; set; } =
        new(DefaultESPOptions.mobOptDefault) { ColorU = ConfigConstants.White };

    public ESPOption.ESPOption PatrolOption { get; set; } =
        new(DefaultESPOptions.mobOptDefault) { ColorU = Color.Yellow };

    public ESPOption.ESPOption ActivatableOption { get; set; } =
        new(DefaultESPOptions.objectOptDefault) { ColorU = Color.Blue };
}
