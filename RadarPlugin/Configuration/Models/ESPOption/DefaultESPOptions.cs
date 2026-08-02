using RadarPlugin.Enums;

namespace RadarPlugin.Configuration.Models.ESPOption;

public static class DefaultESPOptions
{
    public static readonly Models.ESPOption.ESPOption playerOptDefault =
        new Models.ESPOption.ESPOption
        {
            Enabled = true,
            ColorU = 0xffff00ff,
            DisplayType = DisplayTypes.DotAndName,
            DisplayTypeFlags = DisplayTypes.DotAndName.ToFlags(),
            DisplayTypeFlags2D = DisplayTypes.DotAndName.ToFlags(),
            DrawDistance = false,
        };

    public static readonly Models.ESPOption.ESPOption objectOptDefault =
        new Models.ESPOption.ESPOption
        {
            Enabled = true,
            ColorU = 0xffFFFF00,
            DisplayType = DisplayTypes.NameOnly,
            DisplayTypeFlags = DisplayTypes.NameOnly.ToFlags(),
            DisplayTypeFlags2D = DisplayTypes.NameOnly.ToFlags(),
            DrawDistance = false,
        };

    public static readonly Models.ESPOption.ESPOption mobOptDefault = new Models.ESPOption.ESPOption
    {
        Enabled = true,
        ColorU = 0xffffffff,
        DisplayType = DisplayTypes.HealthValueAndName,
        DisplayTypeFlags = DisplayTypes.HealthValueAndName.ToFlags(),
        DisplayTypeFlags2D = DisplayTypes.HealthValueAndName.ToFlags(),
        DrawDistance = false,
    };
}
