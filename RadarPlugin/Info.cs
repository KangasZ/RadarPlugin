using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace RadarPlugin;

public static class Info
{
    public static Dictionary<uint, uint> HuntRecolors = new Dictionary<uint, uint>()
    {
        { 8131, 0xFFFFFFFF } // white - Hydatos Elemental
    };

    public static HashSet<uint> IgnoreList = new HashSet<uint>()
    {
        10261, // carbuncle
        1398, // eos
        1399, // selene
        5478, // ruby carbuncle
        7974, //ball lightning
        7958, //hidden trap
    };
}