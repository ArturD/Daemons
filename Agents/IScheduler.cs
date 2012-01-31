using System;

namespace Daemons
{
    public interface IScheduler : IDisposable
    {
        void Schedule(Action action);
        void ScheduleOne(Action action, TimeSpan delay);
        void ScheduleInterval(Action action, TimeSpan period);
    }
}
