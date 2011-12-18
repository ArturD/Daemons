using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;

namespace Agents
{
    public class DefaultSchedulerDispatcher : IDisposable, IScheduler
    {
        private readonly NoWaitQueue<ProcessAction> _queue = new NoWaitQueue<ProcessAction>();
        private readonly IScheduler _scheduler;
        private int _elementCounter = 0;
        private int _executing = 0;
        private bool _disposed = false;

        public DefaultSchedulerDispatcher(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Schedule(Action action)
        {
            var processAction = new ProcessAction(action);
            if (_disposed) return;

            if (Interlocked.Exchange(ref _executing, 1) == 0)
            {
                // still thread
                action();
                _executing = 0;
            }
            else
            {
                _queue.Add(processAction);

                // If there is no action on global scheduler, we need to add one.
                if (Interlocked.Increment(ref _elementCounter) == 1)
                    _scheduler.Schedule(DoOne);
            }

        }

        private void DoOne()
        {
            if (Interlocked.Exchange(ref _executing, 1) == 0)
            {
                var takeResult = _queue.TakeNoWait();
                if (_disposed) return;
                Debug.Assert(takeResult.Success, "At this point there must have been previous element.");
                takeResult.Value.Execute();
                if (Interlocked.Decrement(ref _elementCounter) != 0) _scheduler.Schedule(DoOne);
                _executing = 0;
            }
            else _scheduler.Schedule(DoOne);
        }


        public void Dispose()
        {
            _disposed = true;
        }

        internal class ProcessAction
        {
            private readonly Action _action;

            public ProcessAction(Action action)
            {
                _action = action;
            }

            public void Execute()
            {
                _action();
            }
        }
    }
}
