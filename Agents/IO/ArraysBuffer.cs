using System;
using System.Collections.Generic;
using System.Linq;

namespace Daemons.IO
{
    public class ArraysBuffer : IBufferReader
    {
        private readonly List<ArraySegment<byte>> _subBuffers = new List<ArraySegment<byte>>();

        public void Add(ArraySegment<byte> segment)
        {
            _subBuffers.Add(segment);
        }

        public void Add(byte[] array)
        {
            Add(new ArraySegment<byte>(array));
        }

        public int Count { get { return _subBuffers.Sum(x => x.Count); } }

        public byte this[int idx]
        {
            get
            {
                for (int i = 0; i < _subBuffers.Count; i++)
                {
                    if (_subBuffers[i].Count < idx)
                    {
                        return _subBuffers[i].Array[_subBuffers[i].Offset + idx];
                    }
                    idx -= _subBuffers[i].Count;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public ArraySegment<byte>? Next
        {
            get { return (_subBuffers.Count > 0) ? _subBuffers[0] : (ArraySegment<byte>?) null; }
        }

        public IEnumerable<ArraySegment<byte>> Take(int bytes)
        {
            while (_subBuffers.Count > 0 && bytes > 0)
            {
                ArraySegment<byte> yieldedSegment;
                if (_subBuffers.Count <= bytes)
                {
                    yieldedSegment = _subBuffers[0];
                    _subBuffers.RemoveAt(0);
                }
                else
                {
                    var current = _subBuffers[0];
                    yieldedSegment = new ArraySegment<byte>(current.Array, current.Offset, bytes);
                    _subBuffers[0] = new ArraySegment<byte>(current.Array, current.Offset + bytes, current.Count - bytes);
                }
                yield return yieldedSegment;
            }
        }
    }
}