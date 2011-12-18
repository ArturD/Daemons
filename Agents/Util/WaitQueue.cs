using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agents.Util
{
    public class WaitQueue<TValue>
    {
        private readonly NoWaitQueue<TValue> _innerQueue = new NoWaitQueue<TValue>();
        //private ManualResetEvent _event = new ManualResetEvent(false);
        private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);
        private int _eventFlag = 0;

        public void Add(TValue value)
        {
            _innerQueue.Add(value);
            _semaphore.Release();
        } 

        public TValue Take()
        {
            _semaphore.WaitOne();
            var result = _innerQueue.TakeNoWait();
            if (result.Success) return result.Value;
            else throw new InvalidAsynchronousStateException("at this point there must be element in queue.");
        }
    }
}
