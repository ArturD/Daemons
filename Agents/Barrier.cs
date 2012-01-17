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
        private readonly NoWaitProducerConsumerCollection<Action> _continuations = new NoWaitProducerConsumerCollection<Action>();  
        private int _counter;
        private int _counter2;

        public Barrier(int counter)
        {
            _counter = counter;
            _counter2 = counter;
        }

        public void Join(Action continuation)
        {
            JoinSync(() => Daemons.Current.Schedule(continuation));
        }

        internal void JoinSync(Action action)
        {
            if (Interlocked.Decrement(ref _counter) < 0) 
                throw new InvalidOperationException("Joined more than expected (counter below 0).");
            _continuations.Add(action);
            if (Interlocked.Decrement(ref _counter2) == 0)
            {
                while (true)
                {
                    Action continuation;
                    if (!_continuations.TryTake(out continuation)) break;
                    continuation();
                }
            }
        }
    }
}
