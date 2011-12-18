using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agents.Util
{
    public class WaitQueue<TValue>
    {
        private readonly NoWaitQueue<TValue> _innerQueue = new NoWaitQueue<TValue>();
        private ManualResetEvent _event = new ManualResetEvent(false);
        private int _eventFlag = 0;

        public void Add(TValue value)
        {
            _innerQueue.Add(value);
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
                    var result = _innerQueue.TakeNoWait();
                    if (result.Success)
                    {
                        return result.Value;
                    }
                }
                _eventFlag = 1;
                _event.Reset();
                // timeout is last line of defence if object is inconsistent
                _event.WaitOne(10000);
            }
        }
    }
}
