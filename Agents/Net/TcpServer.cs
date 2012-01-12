using System;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace Agents.Net
{
    public class TcpServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IProcess _process;
        private readonly IProcessManager _processManager;
        private TcpListener _listener = null;
        private Action<IProcess, TcpConnection> _processInitializator;

        public TcpServer(IProcess process, IProcessManager processManager)
        {
            _process = process;
            _processManager = processManager;
        }

        public void Listen(IPEndPoint endpoint, Action<IProcess, TcpConnection> processInitializator)
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
            var process = _processManager.BuildProcess();
            process.Dispatcher.Schedule(
                () =>
                    {
                        Logger.Trace("Started new TCP process.");
                        var connection = new TcpConnection(client, process);
                        process.OnShutdown(() => connection.Close());
                        _processInitializator(process, connection);
                    });
        }
    }
}
