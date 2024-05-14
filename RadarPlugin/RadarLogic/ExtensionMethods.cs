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

    [Obsolete("Use Vector2.Rotate instead")]
    public static Vector2 RotatedVector(this Vector2 v1, float rotation)
    {
        var cos = Math.Cos(-rotation);
        var sin = Math.Sin(-rotation);
         return new Vector2((float)(v1.X * cos - v1.Y * sin),
            (float)(v1.X * sin + v1.Y * cos));
    }
}