using System;
using System.IO;
using System.Net.Sockets;
using NLog;

namespace Agents.Net
{
    public class TcpConnection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly TcpClient _client;
        private readonly IProcess _process;
        private readonly NetworkStream _stream;

        public TcpConnection(TcpClient client, IProcess process)
        {
            _client = client;
            _process = process;
            _stream = _client.GetStream();
           
        }

        public void ReadAsync(byte[] buffer, int offset, int count, Action<int> endAction)
        {
            if (_stream.DataAvailable)
            {
                Logger.Trace("Reading data synchronously");
                var read = _stream.Read(buffer, offset, count);
                if(Logger.IsTraceEnabled) Logger.Trace("Read data synchronously {0}", read);

                _process.Scheduler.Schedule(
                    () => endAction(read));

            }
            Logger.Trace("Reading data asynchronously");
            try
            {
                _stream.BeginRead(buffer, offset, count,
                                  asyncResult => _process.Scheduler.Schedule(
                                      () => HandleEndRead(endAction, asyncResult)), null);
            }
            catch (IOException exception)
            {
                Logger.TraceException("Connection Reset by Peer", exception);
                _process.Shutdown();
            }
        }

        private void HandleEndRead(Action<int> endAction, IAsyncResult asyncResult)
        {
            int read = 0;
            try
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Ending reading data asynchronously");
                read = _stream.EndRead(asyncResult);
                if (Logger.IsTraceEnabled)
                    Logger.Trace("Read data asynchronously {0}", read);
            }
            catch (IOException exception)
            {
                Logger.TraceException("Connection Reset by Peer", exception);
                _process.Shutdown();
            }
            catch (Exception exception)
            {
                Logger.FatalException(
                    "Exception while ending reading data asynchronously",
                    exception);
                throw;
            }
            if(read == 0) _process.Shutdown();
            else endAction(read);
        }

        public void WriteAsync(byte[] buffer, int offset, int count, Action endAction)
        {
            if (Logger.IsTraceEnabled)
                Logger.Trace("Begining writing data asynchronously");
            try
            {
                _stream.BeginWrite(buffer, offset, count,
                                   asyncResult =>
                                   _process.Scheduler.Schedule(
                                       () =>
                                           {
                                               if (Logger.IsTraceEnabled)
                                                   Logger.Trace("Ending writing data asynchronously");
                                               _stream.EndWrite(asyncResult);
                                               if (Logger.IsTraceEnabled)
                                                   Logger.Trace("Ended writing data asynchronously");
                                               endAction();
                                           }), null);
            }
            catch (IOException exception)
            {
                Logger.TraceException("Connection Reset by Peer", exception);
                _process.Shutdown();
            }
        }

        public bool Connected
        {
            get { return _client.Client.Connected; }
        }

        public IProcess Process
        {
            get { return _process; }
        }

        public void Close()
        {
            Logger.Trace("Closing TCP process.");
            _stream.Dispose();
            _client.Close();
        }
    }
}