using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agents.Util
{
    public class WaitQueue<TValue>
    {
        private readonly IProducerConsumerCollection<TValue> _innerQueue = new ConcurrentQueue<TValue>();
        private ManualResetEvent _event = new ManualResetEvent(false);
        private volatile int _eventFlag = 0;

        public void Add(TValue value)
        {
            _innerQueue.TryAdd(value);
            if (_eventFlag == 1)
            {
                _eventFlag = 0;
                _event.Set();
            }
        } 

        public TValue Take()
        {
            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    TValue result;
                    if (_innerQueue.TryTake(out result))
                    {
                        return result;
                    }
                }
                _eventFlag = 1;
                _event.Reset();
                // timeout is last line of defence if object is inconsistent
                _event.WaitOne(1000);
            }
        }
    }
}
