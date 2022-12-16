using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;

namespace RadarPlugin;

public static class UtilInfo
{
    public static readonly uint Green = 0xFF00FF00;
    public static readonly uint Red = 0xFF0000FF;
    public static readonly uint White = 0xFFFFFFFF;
    public static readonly uint Yellow = 0xFF00FFFF;

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
        14764, // Infern brand 2
    };
    
    /**
     * This list is a dictionary for objects to track along with renames for them
     * KEY: uint representing DATA ID
     * Value: String representing name
     */
    public static Dictionary<uint, string> RenameList = new()
    {
        // Coffers
        {2007358, "Gold Coffer"},
        {802, "Bronze Coffer"},
        {2007357, "Silver Coffer"},
        {2007542, "Accursed Hoard"},
        // Mimics
        {2006020, "Mimic Coffer"},
        //Cairn/Beacons
        {2007187, "Cairn of Return"},
        {2007188, "Cairn of Passage"},
        {2009507, "Beacon of Passage"},
        {2009506, "Beacon of Return"},
        // Traps
        {2007182, "Combustion Trap"},
        {2007183, "Luring Trap"},
        {2007184, "Enervation Trap"},
        {2007185, "Impeding Trap"},
        {2007186, "Toad Trap"},
        {2009504, "Otter Trap"},
        
        // BA Objects
        {2009729, "Portal"}, // Eureka Portal,
        {2009726, "Unstable Portal"},
        {2009727, "Stable Portal"}
    };
    

    public static uint Color(byte r, byte g, byte b, byte a)
    {
        var intColor = (a << 24) | (g << 8) | (b << 16) | r;
        return (uint)intColor;
    }
}