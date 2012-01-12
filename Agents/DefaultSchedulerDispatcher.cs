using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;
using NLog;

namespace Agents
{
    public class DefaultSchedulerDispatcher : IDisposable, IScheduler
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IProcess _process;
        private readonly IProducerConsumerCollection<Action> _queue = new NoWaitQueue<Action>();
        private readonly IScheduler _scheduler;
        private int _elementCounter = 0;
        private volatile int _executing = 0;
        private bool _disposed = false;

        public DefaultSchedulerDispatcher(IScheduler scheduler, IProcess process)
        {
            _scheduler = scheduler;
            _process = process;
        }

        public void Schedule(Action action)
        {
            //var processAction = new ProcessAction(action);
            if (_disposed) return;

            if (_executing == 0 && Daemons.CurrentOrNull == null && Interlocked.Exchange(ref _executing, 1) == 0)
            {
                Logger.Trace("Executing action by stilling thread.");
                // still thread
                using (Daemons.Use(_process))
                {
                    action();
                }
                _executing = 0;
            }
            else
            {
                Logger.Trace("Queueing action.");
                _queue.TryAdd(action);

                // If there is no action on global scheduler, we need to add one.
                _scheduler.Schedule(DoOne);
            }

        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            _scheduler.ScheduleOne(() => Schedule(action), delay);
        }

        public void ScheduleInterval(Action action, TimeSpan delay)
        {
            _scheduler.ScheduleInterval(() => Schedule(action), delay);
        }

        private void DoOne()
        {
            if (Interlocked.Exchange(ref _executing, 1) == 0)
            {
                Action action;
                if (_queue.TryTake(out action))
                {
                    if (_disposed) return;

                    using (Daemons.Use(_process))
                    {
                        action();
                    }
                }
                _executing = 0;
            }
            else _scheduler.Schedule(DoOne);
        }


        public void Dispose()
        {
            _disposed = true;
        }
    }
}
