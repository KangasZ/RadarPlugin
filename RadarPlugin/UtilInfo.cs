using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

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
     * Name ID's to NOT render
     */
    public static HashSet<uint> NameIdIgnoreList = new HashSet<uint>()
    {
        7977, // Relative Virtue
        7958, // Eureka Hidden Trap
        7982, // Shadow in Ozma Fight
        7971, // Munderg on owain
        7974, // Ball Lightning on Raiden
        108, // scarmiglione fight barriers
        7969, //art spears
        10745, // akantha in hesperos fight
        8346, //granite goal in e4s
    };

    /**
     * This is a dictionary to fix bosses that have duplicates on the screen
     * KEY: uint representing name id
     * VALUE: uint representing DATA ID
     */
    public static HashSet<uint> DataIdIgnoreList = new()
    {
        9020, // Titan e4s
        5352, // sephirot
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
    public static Dictionary<uint, string> ObjectTrackList = new()
    {
        {451, "Treasure Coffer"}, 
        {2007358, "Gold Coffer"},
        {802, "Bronze Coffer"},
        {2007188, "Cairn of Passage"},
        {2007357, "Silver Coffer"},
        {2009729, "Portal"}, // Eureka Portal
    };

    /**
     * This is a dictionary to fix bosses that have duplicates on the screen
     * KEY: uint representing name id
     * VALUE: uint representing DATA ID
     */
    public static Dictionary<uint, uint> BossFixList = new()
    {
        { 4776, 5346 }, //sephirot
        { 7968, 9818 }, //ART
        { 7973, 9733 }, // Raiden
        { 7976, 9692 }, // AV
        { 7981, 9704 }, // Ozma
        { 7970, 9821 }, // Owain
        { 11372, 14789 }, // Scarmiglione in Trioa
        { 10744, 13824 }, // Hesperos 2
        { 10742, 13821 }, //hesperos 1
        { 8350, 10639 }, //titan in e4s
    };

    public static uint Color(byte r, byte b, byte g, byte o)
    {
        var intColor = (o << 24) | (g << 16) | (b << 8) | r;
        return (uint)intColor;
    }
}