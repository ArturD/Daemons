using System;
using System.Net;
using System.Net.Sockets;
using Common.Logging;

namespace Daemons.Net
{
    public class LowLevelTcpServer
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        private readonly IDaemonManager _daemonManager;
        private TcpListener _listener = null;
        private Action<IDaemon, LowLevelTcpConnection> _processInitializator;

        public LowLevelTcpServer(IDaemonManager daemonManager)
        {
            _daemonManager = daemonManager;
        }

        public void Listen(IPEndPoint endpoint, Action<IDaemon, LowLevelTcpConnection> processInitializator)
        {
            if(_listener != null)
                throw new InvalidOperationException("Listen Called second time."); // todo make me more verbose

            _listener = new TcpListener(endpoint);
            _processInitializator = processInitializator;
            _listener.Start();
            RunAccept();
        }

        private void RunAccept()
        {
            if (Logger.IsTraceEnabled) Logger.TraceFormat("Begining Accept at {0}", GetHashCode());
            _listener.BeginAcceptSocket(Accept, null);
        }

        private void Accept(IAsyncResult ar)
        {
            if (Logger.IsTraceEnabled) Logger.TraceFormat("Ending Accept at {0}", GetHashCode());
            var tcpClient = _listener.EndAcceptTcpClient(ar);
            BuildNewListenerProcess(tcpClient);
            RunAccept(); // todo consider scheduling it on process
        }

        private void BuildNewListenerProcess(TcpClient client)
        {
            var process = _daemonManager.Spawn(
                (daemon) =>
                    {
                        Logger.Trace("Started new TCP process.");
                        var connection = new LowLevelTcpConnection(client, daemon);
                        daemon.OnShutdown(connection.Close);
                        _processInitializator(daemon, connection);
                    });
        }
    }
}
