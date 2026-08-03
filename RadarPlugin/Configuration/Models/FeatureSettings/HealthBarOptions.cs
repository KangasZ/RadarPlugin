using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class HealthBarOptions
{
    public bool Enabled = true;
    public bool DrawPercent = true;
    public bool DrawName = true;
    public bool CenteredText = false;
    public float YOffset = 20;
    public float YSize = 20f;
    public float XSize = 200f;
    public uint BackgroundColor = Color.Black;
    public uint ProgressColor = Color.Blue;
    public uint BorderColor = Color.Black;
    public uint TextColor = Color.White;
    public float BorderThickness = 0.4f;
}
