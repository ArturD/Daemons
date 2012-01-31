using System;
using Daemons.Reactors;

namespace Daemons
{
    public interface IDaemonManager
    {
        T SpawnWithReactor<T>() where T : IReactor;
        IDaemon Spawn();
        IDaemon Spawn(Action<IDaemon> initAction);
    }
}