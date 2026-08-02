using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class Radar2DConfiguration
{
    public bool Enabled = false;
    public bool ShowBackground = true;
    public uint BackgroundColor = Color.BackgroundDefault;
    public bool Clickthrough = false;
    public bool ShowCross = true;
    public uint CrossColor = Color.White;
    public bool ShowRadarBorder = true;
    public bool ShowSettings = true;
    public bool ShowScale = true;
    public float Scale = 5f;
    public bool ShowYourCurrentPosition = true;
    public bool RotationLockedNorth = false;
    public ConeSettings PlayerConeSettings = new ConeSettings() { ConeColor = Color.Gray50 };
    public ConeSettings CameraConeSettings = new ConeSettings() { ConeColor = Color.LightBlue50 };
}
