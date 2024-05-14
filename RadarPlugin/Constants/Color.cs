namespace RadarPlugin.Constants;

public class Color
{
    //Colors
    // These are all in ABGR format
    public const uint Green = 0xFF00FF00;
    public const uint Red = 0xFF0000FF;
    public const uint Blue = 0xFFFF0000;
    public const uint White = 0xFFFFFFFF;
    public const uint Gold = 0xFF37AFD4;
    public const uint Silver = 0xFFc0c0c0;
    public const uint Yellow = 0xFF00FFFF;
    public const uint Orange = 0xFF00A5FF;
    public const uint LightBlue = 0xFFE6D8AD;
    public const uint Bronze = 0xFF327FCD;
    public const uint Turquoise = 0xffc8d530;
    public const uint Black = 0xff000000;
    public const uint BackgroundDefault = 0xED0F0F0F;
    public const uint Opacity50 = 0x7FFFFFFF;
    public const uint Gray = 0xFF7F7F7F;
    public const uint Gray50 = Gray & Opacity50;
    public const uint LightBlue50 = LightBlue & Opacity50;
}