using System;
using System.Numerics;

namespace RadarPlugin.RadarLogic;

public static class ExtensionMethods
{
    public static float Distance2D(this Vector3 v, Vector3 v2)
    {
        return new Vector2(v.X - v2.X, v.Z - v2.Z).Length();
    }

    public static bool FuzzyEquals(this float a, float b, float tolerance = 0.001f)
    {
        return Math.Abs(a - b) <= tolerance;
    }
}