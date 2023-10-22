using System.Collections.Generic;
using System.Numerics;
using RadarPlugin.Enums;

namespace RadarPlugin;

public static class UtilInfo
{
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
    public const float MinDotSize = 0.6f;
    public const float MaxDotSize = 6f;
    public const uint OpacityMax = 0xff000000;
    public const float DefaultDotSize = 2.2f;
    public const float DefaultEspPadding = 7f;
    public const float DefaultMaxEspDistance = 300f;
    public const float DefaultMaxAggroRadiusDistance = 50f;


    /**
     */
    public static Dictionary<uint, float> AggroDistance = new Dictionary<uint, float>()
    {
        { 5832, 16f } //mimic 2
    };

    public static HashSet<ushort> DeepDungeonMapIds = new()
    {
        561, 562, 563, 564, 565, 593, 594, 595, 596, 597, 598, 599, 600, 601, 602, 603, 604, 605, // POTD
        770, 771, 772, 782, 773, 783, 774, 784, 775, 785, // HOH
        732, 763, 795, 827, // Eureka (Old) (in order)
        1099, 1100, 1101, 1102, 1103, 1104, 1105, 1106, 1107, 1108, // Eureka Orthos in order
    };

    /**
     * This is a dictionary to fix bosses that have duplicates on the screen
     * KEY: uint representing name id
     * VALUE: uint representing DATA ID
     */
    public static HashSet<uint> DataIdIgnoreList = new()
    {
        9020, // a bunch of stuff
        6388, // Opened traps in potd
        /*5352, // sephirot
        14816, // sephirot
        14835, // silkie
        14836, // eastern ewer
        14766, // Infern brand
        14764, // Infern brand 2
        9823, // Owain
        9822, // munderg
        9841, // owain
        9824, // owain
        9768, // Av?*/ 
    };

    /**
     * This list is a dictionary for objects to track along with renames for them
     * KEY: uint representing DATA ID
     * Value: String representing name
     */
    public static Dictionary<uint, string> RenameList = new()
    {
        // Coffers
        { 2007358, "Gold Coffer" },
        { 2007357, "Silver Coffer" },
        { 2007542, "Accursed Hoard" },

        // Potd Bronze Coffers
        { 782, "Bronze Coffer" },
        { 783, "Bronze Coffer" },
        { 784, "Bronze Coffer" },
        { 785, "Bronze Coffer" },
        { 786, "Bronze Coffer" },
        { 787, "Bronze Coffer" },
        { 788, "Bronze Coffer" },
        { 789, "Bronze Coffer" },
        { 790, "Bronze Coffer" },
        { 802, "Bronze Coffer" },
        { 803, "Bronze Coffer" },
        { 804, "Bronze Coffer" },
        { 805, "Bronze Coffer" },

        // Hoh bronze coffers
        { 1036, "Bronze Coffer" },
        { 1037, "Bronze Coffer" },
        { 1038, "Bronze Coffer" },
        { 1039, "Bronze Coffer" },
        { 1040, "Bronze Coffer" },
        { 1041, "Bronze Coffer" },
        { 1042, "Bronze Coffer" },
        { 1043, "Bronze Coffer" },
        { 1044, "Bronze Coffer" },
        { 1045, "Bronze Coffer" },
        { 1046, "Bronze Coffer" },
        { 1047, "Bronze Coffer" },
        { 1048, "Bronze Coffer" },
        { 1049, "Bronze Coffer" },

        // Eureka Orthos Bronze Coffers
        { 1541, "Bronze Coffer" },
        { 1542, "Bronze Coffer" },
        { 1543, "Bronze Coffer" },
        { 1544, "Bronze Coffer" },
        { 1545, "Bronze Coffer" },
        { 1546, "Bronze Coffer" },
        { 1547, "Bronze Coffer" },
        { 1548, "Bronze Coffer" },
        { 1549, "Bronze Coffer" },
        { 1550, "Bronze Coffer" },
        { 1551, "Bronze Coffer" },
        { 1552, "Bronze Coffer" },
        { 1553, "Bronze Coffer" },
        { 1554, "Bronze Coffer" },

        // Mimics
        { 2006020, "Mimic Coffer" },
        { 2566, "Mimic" },
        { 5832, "Mimic" },
        { 7392, "Bronze Mimic" },
        { 7393, "Silver Mimic" },
        { 7394, "Gold Mimic" },

        // Orthos
        { 15997, "Bronze Mimic" },
        { 15998, "Bronze? Mimic" },
        { 15999, "Silver Mimic" },
        { 16002, "Gold Mimic" },

        // Traps
        { 2007182, "Landmine" },
        { 2007183, "Luring Trap" },
        { 2007184, "Enfeebling Trap" },
        { 2007185, "Impeding Trap" },
        { 2007186, "Toading Trap" },
        { 2009504, "Odder Trap" },
        { 2013284, "Owlet Trap" },

        // BA Objects
        { 2009728, "Trap" }, // Eureka Portal,
        { 2009729, "Portal" }, // Eureka Portal,
        { 2009726, "Unstable Portal" },
        { 2009727, "Stable Portal" }
    };

    public static Dictionary<uint, DeepDungeonMobTypes> DeepDungeonMobTypesMap = new()
    {
        // Mimics
        { 2006020, DeepDungeonMobTypes.Mimic },
        { 2566, DeepDungeonMobTypes.Mimic },
        { 7392, DeepDungeonMobTypes.Mimic },
        { 7393, DeepDungeonMobTypes.Mimic },
        { 7394, DeepDungeonMobTypes.Mimic },
        { 5832, DeepDungeonMobTypes.Mimic },
        { 15997, DeepDungeonMobTypes.Mimic },
        { 15998, DeepDungeonMobTypes.Mimic },
        { 15999, DeepDungeonMobTypes.Mimic },
        { 16000, DeepDungeonMobTypes.Mimic },
        { 16001, DeepDungeonMobTypes.Mimic },
        { 16002, DeepDungeonMobTypes.Mimic },
        { 16003, DeepDungeonMobTypes.Mimic },
        { 16004, DeepDungeonMobTypes.Mimic },
        { 16005, DeepDungeonMobTypes.Mimic },

        // Returns
        { 2007187, DeepDungeonMobTypes.Return },
        { 2009506, DeepDungeonMobTypes.Return },
        { 2013286, DeepDungeonMobTypes.Return },

        // Passage
        { 2007188, DeepDungeonMobTypes.Passage },
        { 2009507, DeepDungeonMobTypes.Passage },
        { 2013287, DeepDungeonMobTypes.Passage },

        // Bronze Coffers
        // Potd Bronze Coffers
        { 782, DeepDungeonMobTypes.BronzeChest },
        { 783, DeepDungeonMobTypes.BronzeChest },
        { 784, DeepDungeonMobTypes.BronzeChest },
        { 785, DeepDungeonMobTypes.BronzeChest },
        { 786, DeepDungeonMobTypes.BronzeChest },
        { 787, DeepDungeonMobTypes.BronzeChest },
        { 788, DeepDungeonMobTypes.BronzeChest },
        { 789, DeepDungeonMobTypes.BronzeChest },
        { 790, DeepDungeonMobTypes.BronzeChest },
        { 802, DeepDungeonMobTypes.BronzeChest },
        { 803, DeepDungeonMobTypes.BronzeChest },
        { 804, DeepDungeonMobTypes.BronzeChest },
        { 805, DeepDungeonMobTypes.BronzeChest },

        // Hoh bronze coffers
        { 1036, DeepDungeonMobTypes.BronzeChest },
        { 1037, DeepDungeonMobTypes.BronzeChest },
        { 1038, DeepDungeonMobTypes.BronzeChest },
        { 1039, DeepDungeonMobTypes.BronzeChest },
        { 1040, DeepDungeonMobTypes.BronzeChest },
        { 1041, DeepDungeonMobTypes.BronzeChest },
        { 1042, DeepDungeonMobTypes.BronzeChest },
        { 1043, DeepDungeonMobTypes.BronzeChest },
        { 1044, DeepDungeonMobTypes.BronzeChest },
        { 1045, DeepDungeonMobTypes.BronzeChest },
        { 1046, DeepDungeonMobTypes.BronzeChest },
        { 1047, DeepDungeonMobTypes.BronzeChest },
        { 1048, DeepDungeonMobTypes.BronzeChest },
        { 1049, DeepDungeonMobTypes.BronzeChest },

        // Eureka Orthos Bronze Coffers
        { 1541, DeepDungeonMobTypes.BronzeChest },
        { 1542, DeepDungeonMobTypes.BronzeChest },
        { 1543, DeepDungeonMobTypes.BronzeChest },
        { 1544, DeepDungeonMobTypes.BronzeChest },
        { 1545, DeepDungeonMobTypes.BronzeChest },
        { 1546, DeepDungeonMobTypes.BronzeChest },
        { 1547, DeepDungeonMobTypes.BronzeChest },
        { 1548, DeepDungeonMobTypes.BronzeChest },
        { 1549, DeepDungeonMobTypes.BronzeChest },
        { 1550, DeepDungeonMobTypes.BronzeChest },
        { 1551, DeepDungeonMobTypes.BronzeChest },
        { 1552, DeepDungeonMobTypes.BronzeChest },
        { 1553, DeepDungeonMobTypes.BronzeChest },
        { 1554, DeepDungeonMobTypes.BronzeChest },


        // Coffers
        { 2007358, DeepDungeonMobTypes.GoldChest },
        { 2007357, DeepDungeonMobTypes.SilverChest },
        { 2007542, DeepDungeonMobTypes.AccursedHoard },
        { 2007543, DeepDungeonMobTypes.AccursedHoard },

        // Traps
        { 2007182, DeepDungeonMobTypes.Traps },
        { 2007183, DeepDungeonMobTypes.Traps },
        { 2007184, DeepDungeonMobTypes.Traps },
        { 2007185, DeepDungeonMobTypes.Traps },
        { 2007186, DeepDungeonMobTypes.Traps },
        { 2009504, DeepDungeonMobTypes.Traps },
        { 2013284, DeepDungeonMobTypes.Traps },

        //Easy Mobs
        { 5041, DeepDungeonMobTypes.EasyMobs }, // Pygmaioi
        { 7610, DeepDungeonMobTypes.EasyMobs }, // Korrigan

        // Orthos Korrigans
        { 16006, DeepDungeonMobTypes.EasyMobs },
        { 16007, DeepDungeonMobTypes.EasyMobs },
        { 16008, DeepDungeonMobTypes.EasyMobs },
        { 16009, DeepDungeonMobTypes.EasyMobs },
        { 16010, DeepDungeonMobTypes.EasyMobs },
        { 16011, DeepDungeonMobTypes.EasyMobs },
        { 16012, DeepDungeonMobTypes.EasyMobs },
        { 16013, DeepDungeonMobTypes.EasyMobs },
        { 16014, DeepDungeonMobTypes.EasyMobs },
        { 16015, DeepDungeonMobTypes.EasyMobs },


        // Auspices
        // First three may be wrong
        { 7396, DeepDungeonMobTypes.Auspice }, // Komainu
        { 7397, DeepDungeonMobTypes.Auspice }, // Inugami
        { 7398, DeepDungeonMobTypes.Auspice }, // Senri

        { 15898, DeepDungeonMobTypes.Auspice }, // WHM Demiclone
        { 15899, DeepDungeonMobTypes.Auspice }, // BLM Demiclone
        { 15900, DeepDungeonMobTypes.Auspice }, // Onion knight Demiclone


        // POTD Specail Undead
        { 5049, DeepDungeonMobTypes.SpecialUndead },
        { 5048, DeepDungeonMobTypes.SpecialUndead },
        { 5047, DeepDungeonMobTypes.SpecialUndead },
        { 5050, DeepDungeonMobTypes.SpecialUndead },
        { 5052, DeepDungeonMobTypes.SpecialUndead },
        { 5051, DeepDungeonMobTypes.SpecialUndead },
        { 5053, DeepDungeonMobTypes.SpecialUndead },
        { 5046, DeepDungeonMobTypes.SpecialUndead },
        { 5290, DeepDungeonMobTypes.SpecialUndead },
        { 5291, DeepDungeonMobTypes.SpecialUndead },
        { 5293, DeepDungeonMobTypes.SpecialUndead },
        { 5292, DeepDungeonMobTypes.SpecialUndead },
        { 5294, DeepDungeonMobTypes.SpecialUndead },
        { 5295, DeepDungeonMobTypes.SpecialUndead },
        { 5296, DeepDungeonMobTypes.SpecialUndead },
        { 5297, DeepDungeonMobTypes.SpecialUndead },
        { 5298, DeepDungeonMobTypes.SpecialUndead },
        { 5283, DeepDungeonMobTypes.SpecialUndead },
        { 5284, DeepDungeonMobTypes.SpecialUndead },
        { 5285, DeepDungeonMobTypes.SpecialUndead },
        { 5286, DeepDungeonMobTypes.SpecialUndead },
        { 5287, DeepDungeonMobTypes.SpecialUndead },
        { 5288, DeepDungeonMobTypes.SpecialUndead },
        { 5289, DeepDungeonMobTypes.SpecialUndead },

        // Dread Orthos Mobs 
        // Meracydian Clone
        { 15912, DeepDungeonMobTypes.SpecialUndead },
        { 15913, DeepDungeonMobTypes.SpecialUndead },
        { 15914, DeepDungeonMobTypes.SpecialUndead },
        { 15915, DeepDungeonMobTypes.SpecialUndead },
        { 15916, DeepDungeonMobTypes.SpecialUndead },
        { 15917, DeepDungeonMobTypes.SpecialUndead },
        { 15918, DeepDungeonMobTypes.SpecialUndead },
        { 15919, DeepDungeonMobTypes.SpecialUndead },
        { 15920, DeepDungeonMobTypes.SpecialUndead },
        { 15921, DeepDungeonMobTypes.SpecialUndead },

        // Demi Cochma
        { 15902, DeepDungeonMobTypes.SpecialUndead },
        { 15903, DeepDungeonMobTypes.SpecialUndead },
        { 15904, DeepDungeonMobTypes.SpecialUndead },
        { 15905, DeepDungeonMobTypes.SpecialUndead },
        { 15906, DeepDungeonMobTypes.SpecialUndead },
        { 15907, DeepDungeonMobTypes.SpecialUndead },
        { 15908, DeepDungeonMobTypes.SpecialUndead },
        { 15909, DeepDungeonMobTypes.SpecialUndead },
        { 15910, DeepDungeonMobTypes.SpecialUndead },
        { 15911, DeepDungeonMobTypes.SpecialUndead },

        // Lamia Queen
        { 15922, DeepDungeonMobTypes.SpecialUndead },
        { 15923, DeepDungeonMobTypes.SpecialUndead },
        { 15924, DeepDungeonMobTypes.SpecialUndead },
        { 15925, DeepDungeonMobTypes.SpecialUndead },
        { 15926, DeepDungeonMobTypes.SpecialUndead },
        { 15927, DeepDungeonMobTypes.SpecialUndead },
        { 15928, DeepDungeonMobTypes.SpecialUndead },
        { 15929, DeepDungeonMobTypes.SpecialUndead },
        { 15930, DeepDungeonMobTypes.SpecialUndead },
        { 15931, DeepDungeonMobTypes.SpecialUndead },

        // BA Objects
        { 2009728, DeepDungeonMobTypes.Traps }, // Eureka Trap,
        { 2009729, DeepDungeonMobTypes.Passage }, // Eureka Portal,
        { 2009726, DeepDungeonMobTypes.Passage }, // Unstable portal
        { 2009727, DeepDungeonMobTypes.Passage }, // Stable portal
    };

    public static float Distance2D(this Vector3 v, Vector3 v2)
    {
        return new Vector2(v.X - v2.X, v.Z - v2.Z).Length();
    }
}