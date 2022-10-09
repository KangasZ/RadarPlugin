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

    /**
     * This list is a dictionary for objects to track along with renames for them
     * KEY: uint representing DATA ID
     * Value: String representing name
     */
    public static Dictionary<uint, string> ObjectTrackList = new()
    {
        {451, "Treasure Coffer"}, 
        {2007358, "Gold Coffer"},
        {802, "Bronze Coffer"},
        {2007188, "Cairn of Passage"},
        {2007357, "Silver Coffer"},
    };

    /**
     * This is a dictionary to fix bosses that have duplicates on the screen
     * KEY: uint representing name id
     * VALUE: uint representing DATA ID
     */
    public static Dictionary<uint, uint> BossFixList = new()
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