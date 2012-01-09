using System;

namespace Agents
{
    public class ProcessDispatcherWrapper : IScheduler
    {
        private readonly IScheduler _scheduler;
        private readonly IProcess _process;

        public ProcessDispatcherWrapper(IScheduler scheduler, IProcess process)
        {
            _scheduler = scheduler;
            _process = process;
        }

        public void Dispose()
        {
            _scheduler.Dispose();
        }

        public void Schedule(Action action)
        {
            _scheduler.Schedule(BuildWrappingAction(action));
        }

        private Action BuildWrappingAction(Action action)
        {
            return () =>
                       {
                           using (Daemons.Use(_process))
                           {
                               action();
                           }
                       };
        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            _scheduler.ScheduleOne(BuildWrappingAction(action), delay);
        }

        public void ScheduleInterval(Action action, TimeSpan delay)
        {
            _scheduler.ScheduleInterval(BuildWrappingAction(action), delay);
        }
    }
}