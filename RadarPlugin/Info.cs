using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace RadarPlugin;

public static class Info
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
}