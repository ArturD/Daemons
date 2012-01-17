using System;

namespace Agents
{
    public interface IDaemon : IScheduler
    {
        void OnShutdown(Action shutdownAction);
        void Shutdown();
    }
}