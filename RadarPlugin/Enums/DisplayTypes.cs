using System;

namespace RadarPlugin.Enums;

public enum DisplayTypes
{
    DotOnly = 0,
    NameOnly = 1,
    DotAndName = 2,
    HealthBarOnly = 3,
    HealthBarAndValue = 4,
    HealthBarAndName = 5,
    HealthBarAndValueAndName = 6,
    HealthValueOnly = 7,
    HealthValueAndName = 8,
    Custom = 9,
}

[Flags]
public enum DisplayTypeFlags
{
    Default = 0,
    Dot = 1 << 0,
    Name = 1 << 1,
    HealthCircle = 1 << 2,
    HealthValue = 1 << 3,
    Distance = 1 << 4,
    Position = 1 << 5,
    HealthBar = 1 << 6,
}

public static class DisplayTypeExtensions
{
    public static DisplayTypeFlags ToFlags(this DisplayTypes displayType, bool? drawDistance = null)
    {
        var flags = displayType switch
        {
            DisplayTypes.DotOnly => DisplayTypeFlags.Dot,
            DisplayTypes.NameOnly => DisplayTypeFlags.Name,
            DisplayTypes.DotAndName => DisplayTypeFlags.Dot | DisplayTypeFlags.Name,
            DisplayTypes.HealthBarOnly => DisplayTypeFlags.HealthCircle,
            DisplayTypes.HealthBarAndValue => DisplayTypeFlags.HealthCircle
                | DisplayTypeFlags.HealthValue,
            DisplayTypes.HealthBarAndName => DisplayTypeFlags.HealthCircle | DisplayTypeFlags.Name,
            DisplayTypes.HealthBarAndValueAndName => DisplayTypeFlags.HealthCircle
                | DisplayTypeFlags.HealthValue
                | DisplayTypeFlags.Name,
            DisplayTypes.HealthValueOnly => DisplayTypeFlags.HealthValue,
            DisplayTypes.HealthValueAndName => DisplayTypeFlags.HealthValue | DisplayTypeFlags.Name,
            _ => DisplayTypeFlags.Default,
        };

        if (drawDistance.HasValue)
        {
            flags.SetFlag(DisplayTypeFlags.Distance, drawDistance.Value);
        }

        return flags;
    }

    public static DisplayTypes ToDisplayTypes(this DisplayTypeFlags flags)
    {
        DisplayTypes result;

        if (
            flags.HasFlag(DisplayTypeFlags.Dot)
            && flags.HasFlag(DisplayTypeFlags.Name)
            && flags.HasFlag(DisplayTypeFlags.HealthCircle)
            && flags.HasFlag(DisplayTypeFlags.HealthValue)
        )
        {
            result = DisplayTypes.HealthBarAndValueAndName;
        }
        else if (flags.HasFlag(DisplayTypeFlags.Dot) && flags.HasFlag(DisplayTypeFlags.Name))
        {
            result = DisplayTypes.DotAndName;
        }
        else if (
            flags.HasFlag(DisplayTypeFlags.HealthCircle) && flags.HasFlag(DisplayTypeFlags.Name)
        )
        {
            result = DisplayTypes.HealthBarAndName;
        }
        else if (
            flags.HasFlag(DisplayTypeFlags.HealthCircle)
            && flags.HasFlag(DisplayTypeFlags.HealthValue)
        )
        {
            result = DisplayTypes.HealthBarAndValue;
        }
        else if (
            flags.HasFlag(DisplayTypeFlags.HealthValue) && flags.HasFlag(DisplayTypeFlags.Name)
        )
        {
            result = DisplayTypes.HealthValueAndName;
        }
        else if (flags.HasFlag(DisplayTypeFlags.Dot))
        {
            result = DisplayTypes.DotOnly;
        }
        else if (flags.HasFlag(DisplayTypeFlags.Name))
        {
            result = DisplayTypes.NameOnly;
        }
        else if (flags.HasFlag(DisplayTypeFlags.HealthCircle))
        {
            result = DisplayTypes.HealthBarOnly;
        }
        else if (flags.HasFlag(DisplayTypeFlags.HealthValue))
        {
            result = DisplayTypes.HealthValueOnly;
        }
        else
        {
            result = DisplayTypes.Custom;
        }

        // Check if the flags match up perfectly
        if (result.ToFlags() != flags)
        {
            return DisplayTypes.Custom;
        }

        return result;
    }

    public static void SetFlag(this ref DisplayTypeFlags flags, DisplayTypeFlags flag, bool value)
    {
        if (value)
        {
            flags |= flag;
        }
        else
        {
            flags &= ~flag;
        }
    }
}
