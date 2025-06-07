using System;

namespace RadarPlugin.RadarLogic.Modules;

public interface IModuleInterface : IDisposable
{
    public abstract void StartTick();
    public abstract void EndTick();
}
