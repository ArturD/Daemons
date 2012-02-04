using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons.Util
{
    //public class FixedCyclicBuffer
    //{
    //    private readonly byte[] _buffer;
    //    private int _readPosition;
    //    private int _writePosition;

    //    public FixedCyclicBuffer(byte[] buffer)
    //    {
    //        _buffer = buffer;
    //    }

    //    public FixedCyclicBuffer(int size) 
    //        : this(new byte[size])
    //    {
    //    }

    //    public FixedCyclicBuffer()
    //        : this(2048)
    //    {
    //    }

    //    public ArraySegment<byte> NextWritableSegment { get; set; }

    //    public void ChunkWritten(int start, int count)
    //    {
    //        if(start != _writePosition) throw new InvalidOperationException();
    //        _writePosition += count;
    //        if (_writePosition == _buffer.Length) _writePosition = 0;
    //    }

    //    public ArraySegment<> 
    //}
}
