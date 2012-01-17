using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Agents.Util
{
    public class NoWaitProducerConsumerCollection<TValue> : IProducerConsumerCollection<TValue>
    {
        protected Node First = new Node();
        protected Node Last;

        public NoWaitProducerConsumerCollection()
        {
            Last = First;
        }

        public void Add(TValue value)
        {
            var node = new Node {Value = value};
            
            while (true)
            {
                var last = Last;
                if (Interlocked.CompareExchange(ref last.Next, node, null) == null)
                {
                    //Interlocked.CompareExchange(ref _last, last.Next, last); 
                    Last = last.Next; // TODO can leed to race conditions, but that's ok
                    return;
                }
                Interlocked.CompareExchange(ref Last, last.Next, last);
            }
        }

        public bool TryTake(out TValue val)
        {
            while (true)
            {
                var preFirst = First;
                var first = preFirst.Next;
                if (first == null)
                {
                    val = default(TValue);
                    return false;
                }
                if (Interlocked.CompareExchange(ref First, first, preFirst) == preFirst)
                {
                    val = first.Value;
                    return true;
                }
            }
        }

        public bool Any()
        {
            return First.Next != null;
        }

        public IEnumerable<TValue> Take(int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                TValue ret;
                if (TryTake(out ret))
                    yield return ret;
                else 
                    yield break;
            }
        }

        public class Node
        {
            public TValue Value { get; set; }
            internal Node Next;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            TValue val;
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
