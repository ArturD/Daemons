using System;
using System.IO;
using System.Net.Sockets;
using Common.Logging;

namespace Daemons.Net
{
    public class LowLevelTcpConnection
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        private readonly TcpClient _client;
        private readonly IDaemon _daemon;
        private readonly NetworkStream _stream;

        public TcpClient Client
        {
            get { return _client; }
        }

        public NetworkStream Stream
        {
            get { return _stream; }
        }

        public LowLevelTcpConnection(TcpClient client, IDaemon daemon)
        {
            _client = client;
            _daemon = daemon;
            _stream = _client.GetStream();
           
        }

        public void ReadAsync(byte[] buffer, int offset, int count, Action<int> endAction)
        {
            if (_stream.DataAvailable)
            {
                Logger.Trace("Reading data synchronously");
                var read = _stream.Read(buffer, offset, count);
                if(Logger.IsTraceEnabled) Logger.TraceFormat("Read data synchronously {0}", read);

                _daemon.Schedule(
                    () => endAction(read));

            }
            Logger.Trace("Reading data asynchronously");
            try
            {
                _stream.BeginRead(buffer, offset, count,
                                  asyncResult => _daemon.Schedule(
                                      () => HandleEndRead(endAction, asyncResult)), null);
            }
            catch (IOException exception)
            {
                Logger.Debug("Connection Reset by Peer", exception);
                _daemon.Shutdown();
            }
        }

        private void HandleEndRead(Action<int> endAction, IAsyncResult asyncResult)
        {
            int read = 0;
            try
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Ending reading data asynchronously");
                read = _stream.EndRead(asyncResult);
                
                Logger.TraceFormat("Read data asynchronously {0}", read);
            }
            catch (IOException exception)
            {
                Logger.Debug("Connection Reset by Peer", exception);
                _daemon.Shutdown();
            }
            catch (Exception exception)
            {
                Logger.Fatal(
                    "Exception while ending reading data asynchronously",
                    exception);
                throw;
            }
            if(read == 0) _daemon.Shutdown();
            else endAction(read);
        }

        public void WriteAsync(byte[] buffer, int offset, int count, Action endAction)
        {
            Logger.Trace("Begining writing data asynchronously");
            try
            {
                _stream.BeginWrite(buffer, offset, count,
                                   asyncResult =>
                                   _daemon.Schedule(
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
                Logger.Trace("Connection Reset by Peer", exception);
                _daemon.Shutdown();
            }
            catch (ObjectDisposedException exception)
            {
                Logger.Trace("Object disposed.", exception); // TODO think about this
            }
        }

        public bool Connected
        {
            get { return _client.Client.Connected; }
        }

        public IDaemon Daemon
        {
            get { return _daemon; }
        }

        public void Close()
        {
            Logger.Trace("Closing TCP process.");
            _stream.Dispose();
            _client.Close();
        }
    }
}