using System;
using System.Threading;
using Daemons.Util;

namespace Daemons
{
    public class ThreadPoolDaemon : IDaemon
    {
        private readonly NoWaitProducerUnsafeConsumerCollection<Action> _actionQueue
            = new NoWaitProducerUnsafeConsumerCollection<Action>();
        private readonly IScheduler _golbalScheduler;
        private int _scheduled;

        public ThreadPoolDaemon() : this(ThreadPoolScheduler.Instance)
        {
        }

        public ThreadPoolDaemon(IScheduler golbalScheduler)
        {
            if (golbalScheduler == null) throw new ArgumentNullException("golbalScheduler");
            _golbalScheduler = golbalScheduler;
        }

        public void Schedule(Action action)
        {
            _actionQueue.Add(action);
            if (_scheduled == 0 && Interlocked.Exchange(ref _scheduled, 1) == 0)
            {
                if (Daemons.CurrentOrNull == null)
                {
                    PartialFlush(); // agresive optimisation, if this thread is not used by other Daemon, abuse it.
                }
                else
                    _golbalScheduler.Schedule(PartialFlush);
            }
        }

        private void PartialFlush()
        {
            ConsumeSomeActions();
            _scheduled = 0;
            // rare case, still possible due race conditions
            Thread.MemoryBarrier();
            if (_actionQueue.Any() && _scheduled == 0 && Interlocked.Exchange(ref _scheduled, 1) == 0)
            {
                _golbalScheduler.Schedule(PartialFlush);
            }
        }

        private void ConsumeSomeActions()
        {
            foreach (var action in _actionQueue.Take(10))
            {
                ExecuteActionInDaemonContext(action);
            }
        }

        private void ExecuteActionInDaemonContext(Action action)
        {
            using (Daemons.Use(this))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (!TryHandleException(ex)) throw;
                }
            }
        }

        protected virtual bool TryHandleException(Exception exception)
        {
            return false; // todo
        }


        public IDisposable ScheduleOne(Action action, TimeSpan delay)
        {
            return ScheduleInterval(action, delay, TimeSpan.FromMilliseconds(-1));
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan period)
        {
            return ScheduleInterval(action, period, period);
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan dueTime, TimeSpan period)
        {
            return _golbalScheduler.ScheduleInterval(() => Schedule(action), dueTime, period);
        }

        public void OnShutdown(Action shutdownAction)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Shutdown();
        }
    }
}
