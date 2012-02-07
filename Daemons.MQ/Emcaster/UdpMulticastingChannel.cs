using System;
using System.Collections.Generic;
using Daemons.MQ.Emcaster.Sockets;
using Emcaster.Sockets;
using Emcaster.Topics;

namespace Daemons.MQ.Emcaster
{
    public class UdpMulticastingChannel : EmMulticastingChannelBase
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>(); 
        public UdpMulticastingChannel(string address, int port)
        {
            MessageParserFactory = new MessageParserFactory();
            MessageParser parser = MessageParserFactory.Create();
            UdpReceiver receiveSocket = new UdpReceiver(address, port);
            receiveSocket.ReceiveEvent += parser.OnBytes;
            receiveSocket.Start();


            var sendSocket = new UdpSource(address, port);
            sendSocket.Start();
            var asyncWriter = new DaemonWritter(sendSocket);
            TopicPublisher = new TopicPublisher(asyncWriter);
            TopicPublisher.Start();

            _disposables.Add(receiveSocket);
            _disposables.Add(sendSocket);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            lock (_disposables)
            {
                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}