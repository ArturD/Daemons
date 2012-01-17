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
        private WaitQueue<DefaultScheduledAction> _actions = new WaitQueue<DefaultScheduledAction>();
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
                action.Execute();
            }
        }

        public void Schedule(Action action)
        {
            var scheduledAction = new DefaultScheduledAction(action);
            _actions.Add(scheduledAction);
            //return scheduledAction;
        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            var timer = new Timer((cb)=> action(), null, delay, new TimeSpan(-1));
        }

        public void ScheduleInterval(Action action, TimeSpan delay)
        {
            ScheduleInterval(action, delay, delay);
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
                _actions.Add(new DefaultScheduledAction(() => { }));
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    internal class DefaultScheduledAction
    {
        private readonly Action _action;

        public DefaultScheduledAction(Action action)
        {
            this._action = action;
        }

        public void Execute()
        {
            _action();
        }
    }
}
