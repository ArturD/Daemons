using System;

namespace Daemons
{
    public interface IDaemon : IScheduler
    {
        void OnShutdown(Action shutdownAction);
        void Shutdown();
    }
}