using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;

namespace Agents
{
    public class Barrier
    {
        private readonly NoWaitQueue<Action> _continuations = new NoWaitQueue<Action>();  
        private volatile int _counter;
        private volatile int _counter2;

        public Barrier(int counter)
        {
            _counter = counter;
            _counter2 = counter;
        }

        public SynchronizedBarrier On(IScheduler scheduler)
        {
            return new SynchronizedBarrier(scheduler, this);
        }

        public SynchronizedBarrier On(IProcess process)
        {
            return new SynchronizedBarrier(process.Scheduler, this);
        }

        internal void Join(Action action)
        {
            if (Interlocked.Decrement(ref _counter) < 0) 
                throw new InvalidOperationException("Joined more than expected (counter below 0).");
            _continuations.Add(action);
            if (Interlocked.Decrement(ref _counter2) == 0)
            {
                while (true)
                {
                    var take = _continuations.TakeNoWait();
                    if (!take.Success) break;
                    take.Value();
                }
            }
        }

        public class SynchronizedBarrier
        {
            private readonly IScheduler _scheduler;
            private readonly Barrier _barrier;

            public SynchronizedBarrier(IScheduler scheduler, Barrier barrier)
            {
                _scheduler = scheduler;
                _barrier = barrier;
            }

            public void Join(Action continuation)
            {
                _barrier.Join(() => _scheduler.Schedule(continuation));
            }
        }
    }
}
