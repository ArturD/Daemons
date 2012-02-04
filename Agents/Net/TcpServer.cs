using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Daemons.IO;
using Daemons.Reactors;

namespace Daemons.Net
{
    public class TcpServer
    {
        private readonly IDaemonManager _daemonManager;
        private TcpListener _listener;
        private Action<TcpClient> _acceptor;

        public TcpServer() 
            : this(DaemonConfig.Default().BuildManager())
        {
        }

        public TcpServer(IDaemonManager daemonManager)
        {
            if (daemonManager == null) throw new ArgumentNullException("daemonManager");
            _daemonManager = daemonManager;
        }

        public void Listen<TReactor>(IPEndPoint endpoint) where TReactor : IStreamReactor
        {
            Listen(endpoint,
                   (tcp) =>
                       {
                           _daemonManager.SpawnWithReactor<TReactor>(reactor => { reactor.Bind(tcp.GetStream()); });
                       });
        }

        public void Listen(IPEndPoint endpoint, Action<TcpClient> acceptor)
        {
            if(_acceptor != null) throw new InvalidOperationException("Cannot call Listen twice");
            _acceptor = acceptor;
            _listener = new TcpListener(endpoint);
            _listener.Start();
            TryAcceptSome();
        }

        private void TryAcceptSome()
        {
            _listener.BeginAcceptTcpClient(
                asyncResult =>
                    {
                        var client = _listener.EndAcceptTcpClient(asyncResult);
                        _acceptor(client);
                        TryAcceptSome();
;                    }, null);
        }
    }
}
