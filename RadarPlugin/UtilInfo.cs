using System.Collections.Generic;
using System.Numerics;

namespace RadarPlugin;

public static class UtilInfo
{
    /**
     * What is this used for again?
     * Oh yeah, thats right, it's not (currently)
     */
    public static Dictionary<uint, Vector4> HuntRecolors = new Dictionary<uint, Vector4>()
    {
        { 8131, new Vector4(1f,1f,1f,1f) } // white - Hydatos Elemental 
    };

    /**
     * This is a dictionary to fix bosses that have duplicates on the screen
     * KEY: uint representing name id
     * VALUE: uint representing DATA ID
     */
    public static HashSet<uint> DataIdIgnoreList = new()
    {
        9020, // a bunch of stuff
        5352, // sephirot
        14816, // sephirot
        14835, // silkie
        14836, // eastern ewer
        14766, // Infern brand
        14764, // Infern brand
    };
    
    /**
     * Strings to render
     */
    public static HashSet<string> ObjectStringList = new()
    {
        "Treasure Coffer",
        "Personal Spoils"
    };

    /**
     * This list is a dictionary for objects to track along with renames for them
     * KEY: uint representing DATA ID
     * Value: String representing name
     */
    public static Dictionary<uint, string> RenameList = new()
    {
        {2007358, "Gold Coffer"},
        {802, "Bronze Coffer"},
        {2007188, "Cairn of Passage"},
        {2007357, "Silver Coffer"},
        {2009729, "Portal"}, // Eureka Portal
    };
    

    public static uint Color(byte r, byte g, byte b, byte a)
    {
        var intColor = (a << 24) | (g << 16) | (b << 8) | r;
        return (uint)intColor;
    }
}