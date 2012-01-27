using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agents.MessageBus;

namespace Agents.Controllers
{
    public abstract class DaemonControllerBase : IController
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
