using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agents.Util
{
    public class NoWaitQueue<TValue>
    {
        private Node _first = new Node();
        private Node _last;

        public NoWaitQueue()
        {
            _last = _first;
        }

        public void Add(TValue value)
        {
            Node node = new Node() {Value = value};
            
            while (true)
            {
                var last = _last;
                if (Interlocked.CompareExchange(ref last.Next, node, null) == null)
                {
                    return;
                }
                Interlocked.CompareExchange(ref _last, last.Next, last);
            }
        }

        public TakeNoWaitResult TakeNoWait()
        {
            while (true)
            {
                var preFirst = _first;
                var first = preFirst.Next;
                if (first == null)
                {
                    return new TakeNoWaitResult()
                               {
                                   Value = default(TValue),
                                   Success = false,
                               };
                }
                if (first.Remove())
                {
                    Interlocked.CompareExchange(ref _first, first, preFirst);
                    return new TakeNoWaitResult()
                               {
                                   Value = first.Value,
                                   Success = true,
                               };
                }
                else
                {
                    // make progres if queue is stale
                    Interlocked.CompareExchange(ref _first, first, preFirst);
                }
            }
        }

        public struct TakeNoWaitResult
        {
            public TValue Value { get; set; }
            public bool Success { get; set; }
        }

        public class Node
        {
            public TValue Value { get; set; }
            internal int OnQueueFlag = 1;
            internal Node Next;

            public bool OnQueue
            {
                get { return OnQueueFlag == 1; }
            }

            public bool Remove()
            {
                return Interlocked.Exchange(ref OnQueueFlag, 0) == 1;
            }
        }
    }

}
