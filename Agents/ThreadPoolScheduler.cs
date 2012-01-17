using System;
using System.Threading;

namespace Agents
{
    public class ThreadPoolScheduler : IScheduler
    {
        private static readonly ThreadPoolScheduler SingletonInstance = new ThreadPoolScheduler();
        private bool _disposed = false;

        public static ThreadPoolScheduler Instance { get { return SingletonInstance; } }

        public void Schedule(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) =>
                                             {
                                                 if (!_disposed)
                                                     action();
                                             });
        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            var timer = new Timer((cb) => action(), null, delay, new TimeSpan(-1));
        }

        public void ScheduleInterval(Action action, TimeSpan period)
        {
            ScheduleInterval(action, period, period);
        }

        public void ScheduleInterval(Action action, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new Timer(ExecuteAction, action, dueTime, period);
        }

        private static void ExecuteAction(object actionAsObject)
        {
            var action = (Action)actionAsObject;
            action();
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}