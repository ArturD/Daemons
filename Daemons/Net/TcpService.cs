using System;
using System.Net;
using System.Net.Sockets;
using Daemons.IO;

namespace Daemons.Net
{
    public class TcpService : ITcpService
    {
        private readonly IDaemonManager _daemonManager;

        public TcpService(IDaemonManager daemonManager)
        {
            _daemonManager = daemonManager;
        }

        public void Connect<T>(IPEndPoint endpoint) where T : IStreamReactor
        {
            Connect<T>(endpoint, e => { throw e; });
        }

        public void Connect<T>(IPEndPoint endpoint, Action<Exception> faild) where T : IStreamReactor
        {
            Connect<T>(endpoint, r=> { }, faild);
        }

        public void Connect<T>(IPEndPoint endpoint, Action<T> success, Action<Exception> faild) where T : IStreamReactor
        {
            TcpClient tcpClient = new TcpClient(endpoint);
            tcpClient.BeginConnect(endpoint.Address,
                                   endpoint.Port,
                                   async =>
                                   {
                                       try
                                       {
                                           tcpClient.EndConnect(async);
                                       }
                                       catch (Exception e)
                                       {
                                           faild(e);
                                           return;
                                       }
                                       _daemonManager.SpawnWithReactor<T>(
                                           reactor =>
                                               {
                                                   reactor.Bind(tcpClient.GetStream());
                                                   success(reactor);
                                               });
                                   }
                                   , null);
        }
    }
}
