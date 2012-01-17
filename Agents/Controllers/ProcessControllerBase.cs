using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agents.MessageBus;

namespace Agents.Controllers
{
    public abstract class ProcessControllerBase : IController
    {
        private IDaemon _daemon;

        protected ProcessControllerBase()
        {
        }

        public IDaemon Daemon
        {
            get { return _daemon; }
            set { _daemon = value; }
        }

        public IScheduler Scheduler
        {
            get { return _daemon; }
        }

        public void OnShutdown(Action shutdownAction)
        {
            _daemon.OnShutdown(shutdownAction);
        }

        public void Schedule(Action action)
        {
            Scheduler.Schedule(action);
        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            Scheduler.ScheduleOne(action, delay);
        }

        public void ScheduleInterval(Action action, TimeSpan delay)
        {
            Scheduler.ScheduleInterval(action, delay);
        }

        public abstract void Initialize();
    }
}
