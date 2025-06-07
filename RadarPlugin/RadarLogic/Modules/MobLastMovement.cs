using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace RadarPlugin.RadarLogic.Modules;

public class MobLastMovement : IModuleInterface
{
    private Dictionary<uint, (Vector4 Position, DateTime Time)> lastMovementDictionary = new();

    public TimeSpan GetTimeElapsedFromMovement(IGameObject gameObject)
    {
        if (lastMovementDictionary.TryGetValue(gameObject.EntityId, out var value))
        {
            // Fuzzy equals on stuff
            if (
                !value.Position.X.FuzzyEquals(gameObject.Position.X)
                || !value.Position.Y.FuzzyEquals(gameObject.Position.Y)
                || !value.Position.Z.FuzzyEquals(gameObject.Position.Z)
                || !value.Position.W.FuzzyEquals(gameObject.Rotation)
            )
            {
                var positionVector = new Vector4(
                    gameObject.Position.X,
                    gameObject.Position.Y,
                    gameObject.Position.Z,
                    gameObject.Rotation
                );
                lastMovementDictionary[gameObject.EntityId] = (positionVector, DateTime.Now);
                return TimeSpan.Zero;
            }
            else
            {
                return DateTime.Now - value.Time;
            }
        }
        else
        {
            var positionVector = new Vector4(
                gameObject.Position.X,
                gameObject.Position.Y,
                gameObject.Position.Z,
                gameObject.Rotation
            );
            lastMovementDictionary.Add(gameObject.EntityId, (positionVector, DateTime.Now));
            return TimeSpan.Zero;
        }
    }

    public void Dispose()
    {
        lastMovementDictionary.Clear();
    }

    public void StartTick()
    {
        // Maybe do (IF NOT AVAILABLE THEN CULL MOB)
        // Idk, needs testing :(
        // It just uses object ids so, theroretically if theres a single frame the object isnt viewable
        // (ie, switching between zones) itll be fine :)
        // Surely... Right?
        //nothing
    }

    public void EndTick()
    {
        foreach (var mobMovement in lastMovementDictionary)
        {
            if (DateTime.Now - mobMovement.Value.Time > TimeSpan.FromSeconds(100))
            {
                lastMovementDictionary.Remove(mobMovement.Key);
            }
        }
    }
}
