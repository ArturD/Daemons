using System;
using System.Threading;
using Daemons.Util;

namespace Daemons
{
    public class ThreadPoolDaemon : IDaemon
    {
        private readonly NoWaitProducerUnsafeConsumerCollection<Action> _actionQueue
            = new NoWaitProducerUnsafeConsumerCollection<Action>();

        private readonly CopyOnWriteList<Timer> _timers = new CopyOnWriteList<Timer>(); 
        private int _scheduled;

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
                    ThreadPool.QueueUserWorkItem(ScheduledPartialFlush);
            }
        }

        private void ScheduledPartialFlush(object state)
        {
            PartialFlush();
        }

        private void PartialFlush()
        {
            ConsumeSomeActions();
            _scheduled = 0;
            // rare case, still possible due race conditions
            Thread.MemoryBarrier();
            if (_actionQueue.Any() && _scheduled == 0 && Interlocked.Exchange(ref _scheduled, 1) == 0)
            {
                ThreadPool.QueueUserWorkItem(ScheduledPartialFlush);
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
            var timer = new Timer(ExecuteOnSchedulerAction, action, dueTime, period);
            _timers.Add(timer);
            return new AnonymousDisposer(() =>
                                             {
                                                 timer.Dispose();
                                                 _timers.Remove(timer);
                                             });
        }

        private void ExecuteOnSchedulerAction(object actionAsObject)
        {
            Schedule(() =>
                     ((Action) actionAsObject)()
                );
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
