using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class ConeSettings
{
    public bool Enabled = false;
    public uint ConeColor = Color.Gray50;
    public bool Fill = true;
    public float Radius = ConfigConstants.DefaultConeRadius;
    public float RadianAngle = ConfigConstants.DefaultConeAngleRadians; // About sqrt(2)/2 or 45 degrees (from each side)
}
