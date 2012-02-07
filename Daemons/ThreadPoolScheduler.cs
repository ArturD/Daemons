using System;
using System.Threading;

namespace Daemons
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

        public IDisposable ScheduleOne(Action action, TimeSpan delay)
        {
            return new Timer((cb) => action(), null, delay, new TimeSpan(-1));
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan period)
        {
            return ScheduleInterval(action, period, period);
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan dueTime, TimeSpan period)
        {
            return new Timer(ExecuteAction, action, dueTime, period);
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