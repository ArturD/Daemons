using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agents.Util
{
    public class NoWaitQueue<TValue> : IProducerConsumerCollection<TValue>
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

        public bool TryTake(out TValue val)
        {
            while (true)
            {
                var preFirst = _first;
                var first = preFirst.Next;
                if (first == null)
                {
                    val = default(TValue);
                    return false;
                }
                if (Interlocked.CompareExchange(ref _first, first, preFirst) == preFirst)
                {
                    val = first.Value;
                    return true;
                }
            }
        }

        public class Node
        {
            public TValue Value { get; set; }
            //internal int OnQueueFlag = 1;
            internal Node Next;

            //public bool OnQueue
            //{
            //    get { return OnQueueFlag == 1; }
            //}

            //public bool Remove()
            //{
            //    if (OnQueueFlag == 0) return false;
            //    return Interlocked.Exchange(ref OnQueueFlag, 0) == 1;
            //}
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            TValue val = default(TValue);
            while(TryTake(out val))
            {
                yield return val;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public void CopyTo(TValue[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(TValue item)
        {
            Add(item);
            return true;
        }

        public TValue[] ToArray()
        {
            return this.AsEnumerable().ToArray();
        }
    }

}
