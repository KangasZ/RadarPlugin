using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class CastBarOptions
{
    public bool Enabled = false;
    public bool Players = false;
    public bool BattleNpcs = true;
    public bool DrawTime = true;
    public float YOffset = 20;
    public float YSize = 20f;
    public float XSize = 200f;
    public uint BackgroundColor = Color.Black;
    public uint ProgressColor = Color.Blue;
    public uint BorderColor = Color.Black;
    public uint TextColor = Color.White;
    public float BorderThickness = 0.4f;
}
