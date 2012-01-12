using System;
using System.IO;

namespace Agents.IO
{
    public static class StreamExtensions
    {
        public static void Read(this Stream  stream, byte[] buffer, int offset, int size, Action<int> continuation)
        {
            var daemon = Daemons.Current;

            stream.BeginRead(buffer, offset, size,
                             asyncResult =>
                                 {
                                     int read = stream.EndRead(asyncResult);
                                     ((IProcess) asyncResult.AsyncState).Dispatcher.Schedule(() => continuation(read));
                                 }, daemon);
        }

        public static void Write(this Stream stream, byte[] buffer, int offset, int size, Action continuation)
        {
            var daemon = Daemons.Current;

            stream.BeginWrite(buffer, offset, size,
                             asyncResult =>
                             {
                                 stream.EndWrite(asyncResult);
                                 ((IProcess)asyncResult.AsyncState).Dispatcher.Schedule(continuation);
                             }, daemon);
        }

        public static void Write(this Stream stream, byte[] buffer, int size, Action continuation)
        {
            Write(stream, buffer, 0, size, continuation);
        }

        public static void Write(this Stream stream, byte[] buffer, Action continuation)
        {
            Write(stream, buffer, 0, buffer.Length, continuation);
        }
    }
}
