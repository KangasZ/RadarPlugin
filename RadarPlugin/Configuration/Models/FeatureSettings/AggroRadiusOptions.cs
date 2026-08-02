using RadarPlugin.Constants;

namespace RadarPlugin.Configuration.Models.FeatureSettings;

public class AggroRadiusOptions
{
    public bool ShowAggroCircle = false;
    public bool ShowAggroCircleInCombat = false;
    public bool ShowAggroCircleOnPlayerHeight = false;
    public bool MaxDistanceCapBool = true;
    public float MaxDistance = ConfigConstants.DefaultMaxAggroRadiusDistance;
    public bool EnableMaxDistanceArcFromPlayer = true;
    public float MaxDistanceArcFromPlayer = ConfigConstants.DefaultMaxArcLengthFromPlayer;
    public uint FrontColor = ConfigConstants.Red;
    public uint RearColor = ConfigConstants.Green;
    public uint RightSideColor = ConfigConstants.Yellow;
    public uint LeftSideColor = ConfigConstants.Yellow;
    public uint FrontConeColor = ConfigConstants.Red;
    public uint CircleOpacity = 0xBEFFFFFF;
    public uint FrontConeOpacity = 0x30FFFFFF;
    public uint SoundAggroColor = ConfigConstants.Turquoise;
    public uint ProximityAggroColor = ConfigConstants.Red;
}
