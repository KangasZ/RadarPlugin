using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class HitboxOptions
{
    public bool HitboxEnabled = false;
    public bool OverrideMobColor = false;
    public uint HitboxColor = Color.Turquoise;
    public float Thickness = 2.2f;

    public bool DrawInsideCircle = false;
    public uint InsideCircleOpacity = 0xffffffff;
    public bool UseDifferentInsideCircleColor = false;
    public uint InsideCircleColor = Color.Turquoise & 0x50ffffff;
}
