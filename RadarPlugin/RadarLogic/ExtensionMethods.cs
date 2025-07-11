using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using RadarPlugin.Enums;

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
        return new Vector2((float)(v1.X * cos - v1.Y * sin), (float)(v1.X * sin + v1.Y * cos));
    }

    public static unsafe ulong GetAccountId(this IGameObject gameObject)
    {
        ulong accountId = 0;

        if (gameObject.ObjectKind != ObjectKind.Player)
            return accountId;
        var clientstructobj = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)
            (void*)gameObject.Address;
        var tempAccountId = clientstructobj->AccountId;
        if (tempAccountId != 0)
        {
            accountId = tempAccountId;
        }
        return accountId;
    }

    public static unsafe ulong GetContentId(this IGameObject gameObject)
    {
        ulong accountId = 0;

        if (gameObject.ObjectKind != ObjectKind.Player)
            return accountId;
        var clientstructobj = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)
            (void*)gameObject.Address;
        var tempAccountId = clientstructobj->ContentId;
        if (tempAccountId != 0)
        {
            accountId = tempAccountId;
        }
        return accountId;
    }

    public static unsafe ulong GetDeobfuscatedAccountId(
        this IGameObject gameObject,
        ulong obfuscatedSelfId,
        uint yourBaseId
    )
    {
        ulong accountId = 0;

        if (gameObject.ObjectKind != ObjectKind.Player)
            return accountId;
        var clientstructobj = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)
            (void*)gameObject.Address;

        if (yourBaseId == 0)
        {
            var tempAccountId = clientstructobj->ContentId;
            if (tempAccountId != 0)
            {
                accountId = tempAccountId;
            }

            return accountId;
        }
        else
        {
            var tempAccountId = gameObject.GetAccountId();
            if (tempAccountId != 0)
            {
                accountId = tempAccountId;
            }
            accountId = DeobfuscateAccountId(obfuscatedSelfId, accountId, yourBaseId);
            return accountId;
        }
    }

    public static uint DeobfuscateAccountId(ulong selfId, ulong otherId, uint yourBaseId)
    {
        var shiftedVal = (selfId ^ otherId) >> 31;
        return (uint)((shiftedVal ^ yourBaseId) & 0xFFFFFFFF);
    }

    public static MobType GetMobType(this IGameObject gameObject)
    {
        switch (gameObject.ObjectKind)
        {
            case ObjectKind.Player:
                return MobType.Player;
            case ObjectKind.BattleNpc:
                return MobType.Character;
        }

        return MobType.Object;
    }
}
