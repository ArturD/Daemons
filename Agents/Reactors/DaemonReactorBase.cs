using System;

namespace Daemons.Reactors
{
    public abstract class DaemonReactorBase : IReactor
    {
        public IDaemon Daemon { get; set; }

        public void OnShutdown(Action shutdownAction)
        {
            Daemon.OnShutdown(shutdownAction);
        }

        public void Schedule(Action action)
        {
            Daemon.Schedule(action);
        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            Daemon.ScheduleOne(action, delay);
        }

        public void ScheduleInterval(Action action, TimeSpan delay)
        {
            Daemon.ScheduleInterval(action, delay);
        }

        public abstract void Initialize();
    }
}
