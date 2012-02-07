using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;

namespace Daemons
{
    public class ThreadPoolScheduler : IScheduler
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        private static readonly ThreadPoolScheduler SingletonInstance = new ThreadPoolScheduler();
        public static ThreadPoolScheduler Instance { get { return SingletonInstance; } }

        private readonly SortedSet<ScheduledAction> _scheduledActions = new SortedSet<ScheduledAction>();
        private bool _disposed = false;
        private Timer _timer;

        public ThreadPoolScheduler()
        {
            _timer = new Timer(Tick, null, 0, 100);
        }

        private void Tick(object state)
        {
            try
            {
                while (true)
                {
                    ScheduledAction min;
                    lock (_scheduledActions)
                    {
                        min = _scheduledActions.Min;
                        if (min == null || min.ScheduleAt >= DateTime.UtcNow)
                            break;
                        _scheduledActions.Remove(min);
                    }
                    if (!min.ExecuteNext()) break;
                    lock (_scheduledActions)
                    {
                        _scheduledActions.Add(min);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("unexpected error while tick",ex);
            }
        }

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
            var scheduledAction = new ScheduledAction(action, delay, delay, 1);

            ScheduleInFuture(scheduledAction);

            return scheduledAction;
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan period)
        {
            return ScheduleInterval(action, period, period);
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan dueTime, TimeSpan period)
        {
            var scheduledAction = new ScheduledAction(action, period, dueTime);

            ScheduleInFuture(scheduledAction);

            return scheduledAction;
        }

        private void ScheduleInFuture(ScheduledAction scheduledAction)
        {
            lock (_scheduledActions)
            {
                _scheduledActions.Add(scheduledAction);
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
            _disposed = true;
        }

        internal class ScheduledAction : IDisposable, IComparable<ScheduledAction>
        {
            private readonly Action _action;
            private readonly TimeSpan _period;
            private DateTime _scheduleAt;
            private int _repeat;

            public ScheduledAction(Action action, TimeSpan period) : this(action, period, period)
            {
            }

            public ScheduledAction(Action action, TimeSpan period, TimeSpan dueTime) : this(action, period, dueTime, int.MaxValue)
            {
            }

            public ScheduledAction(Action action, TimeSpan period, TimeSpan dueTime, int repeat)
            {
                if (period < TimeSpan.FromMilliseconds(50)) period = TimeSpan.FromMilliseconds(50);
                _action = action;
                _period = period;
                _repeat = repeat;
                _scheduleAt = DateTime.UtcNow + dueTime;
            }

            public DateTime ScheduleAt
            {
                get { return _scheduleAt; }
            }

            public bool ExecuteNext()
            {
                if(_repeat <= 0) return false;
                DateTime next = _scheduleAt + _period;
                _action();
                if(_repeat-1 <= 0) return false;
                if(_repeat != int.MaxValue) _repeat--;
                var now = DateTime.UtcNow;
                if(next < now) next = now;
                _scheduleAt = next;
                return true;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                _repeat = 0;
            }

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public int CompareTo(ScheduledAction other)
            {
                if (Equals(other)) return 0;
                if (_scheduleAt.CompareTo(other._scheduleAt) < 0) return -1;
                return 1;
            }

            public override string ToString()
            {
                return string.Format("Period: {0}, ScheduleAt: {1}, Repeat: {2}", _period, _scheduleAt, _repeat);
            }
        }
    }
}