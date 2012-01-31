using System;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace Daemons.Net
{
    public class TcpServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDaemonManager _daemonManager;
        private TcpListener _listener = null;
        private Action<IDaemon, TcpConnection> _processInitializator;

        public TcpServer(IDaemonManager daemonManager)
        {
            _daemonManager = daemonManager;
        }

        public void Listen(IPEndPoint endpoint, Action<IDaemon, TcpConnection> processInitializator)
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
            if (Logger.IsTraceEnabled) Logger.Trace("Begining Accept at {0}", GetHashCode());
            _listener.BeginAcceptSocket(Accept, null);
        }

        private void Accept(IAsyncResult ar)
        {
            if(Logger.IsTraceEnabled) Logger.Trace("Ending Accept at {0}", GetHashCode());
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
                        var connection = new TcpConnection(client, daemon);
                        daemon.OnShutdown(connection.Close);
                        _processInitializator(daemon, connection);
                    });
        }
    }
}
