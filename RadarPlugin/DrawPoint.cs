using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using ImGuiNET;

namespace RadarPlugin;

public class DrawPoint
{
    [NonSerialized]
    private float x;
    [NonSerialized]
    private float y;
    [NonSerialized]
    private float z;
    [NonSerialized]
    private Vector3 Vector3;
    public float X
    {
        get => MathF.Round(x, 3);
        set => x = value;
    }

    public float Y
    {
        get => MathF.Round(y, 3);
        set => y = value;
    }

    public float Z
    {
        get => MathF.Round(z, 3);
        set => z = value;
    }
    
    private static Vector2 dotCenter;
    
    private BattleNpc ObjectDraw;
    public DrawPoint(BattleNpc character)
    {
        ObjectDraw = character;
    }

    public Vector2 DrawUnder()
    {
        /*var pos = ObjectDraw.Position;
        X = pos.X;
        Y = pos.Y;
        Z = pos.Z;
        Vector3 = new Vector3(X, Y, Z);*/
        Vector2 vector2;
        Services.GameGui.WorldToScreen(ObjectDraw.Position, out vector2);
        //PluginLog.Debug($"Creating vector for character: {ObjectDraw.Name} at {X}, {Y}, {Z} : 2D Vector at {vector2.X}, {vector2.Y}");
        dotCenter = new Vector2(vector2.X, vector2.Y);
        return dotCenter;
        //ImGui.GetForegroundDrawList().AddCircleFilled(dotCenter, 5f, 4278190335, 8);
        //ImGui.GetForegroundDrawList().AddCircleFilled(dotCenter, 5f, 4278190335, 9);
    }
}