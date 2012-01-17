using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;

namespace Agents
{
    /// <summary>
    /// Scheduler that targets minimal number of context switch.
    /// </summary>
    public class DefaultScheduler : IScheduler
    {
        private List<Thread> _threads;
        private WaitQueue<Action> _actions = new WaitQueue<Action>();
        private bool _stopping = false;
 
        public DefaultScheduler(int numberOfThreads)
        {
            _threads = new List<Thread>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread th = new Thread(x => Loop());
                _threads.Add(th);
                th.Start();
            }
        }

        public DefaultScheduler()
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

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            var timer = new Timer((cb)=> action(), null, delay, new TimeSpan(-1));
        }

        public void ScheduleInterval(Action action, TimeSpan period)
        {
            ScheduleInterval(action, period, period);
        }

        public void ScheduleInterval(Action action, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new Timer(ExecuteAction, action, dueTime, period);
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
