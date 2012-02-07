using System;

namespace Daemons
{
    public interface IScheduler : IDisposable
    {
        void Schedule(Action action);
        IDisposable ScheduleOne(Action action, TimeSpan delay);
        IDisposable ScheduleInterval(Action action, TimeSpan period);
    }
}
