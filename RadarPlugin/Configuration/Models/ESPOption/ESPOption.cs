using System;
using RadarPlugin.Constants;
using RadarPlugin.Enums;

namespace RadarPlugin.Configuration.Models.ESPOption;

public class ESPOption
{
    public ESPOption() { }

    // Copy Constructor
    public ESPOption(ESPOption espOption)
    {
        Enabled = espOption.Enabled;
        DisplayType = espOption.DisplayType;
        this.DisplayTypeFlags = espOption.DisplayTypeFlags;
        ColorU = espOption.ColorU;
        DrawDistance = espOption.DrawDistance;
        AppendLevelToName = espOption.AppendLevelToName;
    }

    public bool Enabled = true;

    /*
    public bool ShowDot = true;
    public bool ShowHp = false;
    public bool ReplaceDotWithHP = false;
    public bool ShowName = true;*/
    public bool ShowMp = false;

    [Obsolete]
    public DisplayTypes DisplayType = DisplayTypes.NameOnly;
    public uint ColorU = 0xffffffff;
    public bool DrawDistance = false;
    public bool DotSizeOverride = false;
    public float DotSize = ConfigConstants.DefaultDotSize;
    public bool ReplaceWithJobName = false;
    public bool AppendLevelToName = false;
    public DisplayTypeFlags DisplayTypeFlags = DisplayTypeFlags.Default;

    public bool Separate2DOptions = false;
    public DisplayTypeFlags DisplayTypeFlags2D = DisplayTypeFlags.Default;
    public bool DotSizeOverride2D = false;
    public float DotSize2D = ConfigConstants.DefaultDotSize;
}
