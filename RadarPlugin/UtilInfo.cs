using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace RadarPlugin;

public static class UtilInfo
{
    public static Dictionary<uint, Vector4> HuntRecolors = new Dictionary<uint, Vector4>()
    {
        { 8131, new Vector4(1f,1f,1f,1f) } // white - Hydatos Elemental 
    };

    public static HashSet<uint> IgnoreList = new HashSet<uint>()
    {
        10261, // carbuncle
        1398, // eos
        1399, // selene
        5478, // ruby carbuncle
        7974, //ball lightning
        7958, //hidden trap
    }; // TODO: probs not needed (cope)

    public static HashSet<string> ObjectTrackList = new HashSet<string>()
    {
        "Treasure Chest",
    };

    public static Dictionary<uint, uint> BossFixList = new Dictionary<uint, uint>()
    {
        { 2137, 2319 }, //ultima weapon
        { 4776, 5346 }  //sephirot
    };

    public static uint Color(byte r, byte b, byte g, byte o)
    {
        var intColor = (o << 24) | (g << 16) | (b << 8) | r;
        return (uint)intColor;
    }
}