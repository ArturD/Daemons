using System;
using System.Collections.Generic;
using System.Threading;
using Daemons.Util;

namespace Daemons
{
    /// <summary>
    /// Scheduler that targets minimal number of context switch.
    /// </summary>
    public class FixedThreadPoolScheduler : IScheduler
    {
        private List<Thread> _threads;
        private WaitQueue<Action> _actions = new WaitQueue<Action>();
        private bool _stopping = false;
 
        public FixedThreadPoolScheduler(int numberOfThreads)
        {
            _threads = new List<Thread>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread th = new Thread(x => Loop());
                _threads.Add(th);
                th.Start();
            }
        }

        public FixedThreadPoolScheduler()
            : this(Environment.ProcessorCount)
        {
        }

        private void Loop()
        {
            while (true)
            {
                if (_stopping) break;
                var action = _actions.Take();
                if(_stopping) break;
                action();
            }
        }

        public void Schedule(Action action)
        {
            _actions.Add(action);
            //return scheduledAction;
        }

        public IDisposable ScheduleOne(Action action, TimeSpan delay)
        {
            return new Timer((cb)=> action(), null, delay, new TimeSpan(-1));
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan period)
        {
            return ScheduleInterval(action, period, period);
        }

        public IDisposable ScheduleInterval(Action action, TimeSpan dueTime, TimeSpan period)
        {
            return new Timer(ExecuteAction, action, dueTime, period);
        }

        private static void ExecuteAction(object actionObject)
        {
            Action action = (Action) actionObject;
            action();
        }

        public void Stop()
        {
            _stopping = true;
            foreach (var thread in _threads)
            {
                // add dummy action to ensure cycle in thread
                _actions.Add(() => { });
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
