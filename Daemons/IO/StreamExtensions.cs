using System;
using System.IO;
using Common.Logging;

namespace Daemons.IO
{
    public static class StreamExtensions
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public static void Read(this Stream  stream, byte[] buffer, int offset, int size, Action<int> continuation)
        {
            var daemon = Daemons.Current;

            stream.BeginRead(buffer, offset, size,
                             asyncResult =>
                                 {
                                     int read = stream.EndRead(asyncResult);
                                     ((IDaemon) asyncResult.AsyncState).Schedule(() => continuation(read));
                                 }, daemon);
        }

        public static void Write(this Stream stream, byte[] buffer, int offset, int size, Action continuation)
        {
            var daemon = Daemons.Current;
            
            stream.BeginWrite(buffer, offset, size,
                             asyncResult =>
                             {
                                 stream.EndWrite(asyncResult);
                                 ((IDaemon)asyncResult.AsyncState).Schedule(continuation);
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

        public static void Pipe(this Stream source, Stream destination, Action continuation)
        {
            Pipe(source, destination, 4096, continuation);
        }

        public static void Pipe(this Stream source, Stream destination, int bufferSize, Action continuation)
        {
            Pipe(source, destination, new byte[bufferSize], continuation);
        }

        public static void Pipe(this Stream source, Stream destination, Action continuation, Action<Exception> error)
        {
            Pipe(source, destination, 4096, continuation, error);
        }

        public static void Pipe(this Stream source, Stream destination, int bufferSize, Action continuation, Action<Exception> error)
        {
            Pipe(source, destination, new byte[bufferSize], continuation, error);
        }

        public static void Pipe(this Stream source, Stream destination, byte[] buffer, Action continuation)
        {
            Pipe(source, destination, buffer, continuation, (e) => Logger.Warn("Exception occured in pipe.", e));
        }

        public static void Pipe(this Stream source, Stream destination, byte[] buffer, Action continuation, Action<Exception> error)
        {
            var scheduler = Daemons.CurrentScheduler;

            Action scheduledContinuation = () => scheduler.Schedule(continuation);
            Action<Exception> scheduledError = (e) => scheduler.Schedule(() => error(e));

            var streamPipe = new StreamPipe(source, destination, buffer, scheduledContinuation, scheduledError);
            streamPipe.Start();
        }
    }
}
