using System;
using System.Collections.Generic;
using System.Linq;

namespace Daemons.IO
{
    public interface IBufferReader
    {
        int Count { get; }
        byte this[int idx] { get; }
        ArraySegment<byte>? Next { get; }
        IEnumerable<ArraySegment<byte>> Take(int bytes);
    }

    public static class BufferReaderExtensions
    {
        public static byte[] TakeAsArray(this IBufferReader reader, int bytes)
        {
            var segments = reader.Take(bytes).ToList();
            var len = segments.Sum(x => x.Count);
            var buffer = new byte[len];
            int bi = 0;
            foreach (var arraySegment in segments)
            {
                for (int i = 0; i < arraySegment.Count; i++)
                {
                    buffer[bi++] = arraySegment.Array[arraySegment.Offset + i];
                }
            }
            return buffer;
        }
    }
}