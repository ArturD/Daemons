using System;
using System.Collections.Generic;
using Emcaster.Sockets;
using Emcaster.Topics;

namespace Daemons.MQ.Emcaster
{
    public class PgmMulticastingChannel : EmMulticastingChannelBase
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public PgmMulticastingChannel(string address, int port)
        {
            MessageParserFactory = new MessageParserFactory();
            var reader = new PgmReader(MessageParserFactory);
            var receiveSocket = new PgmReceiver(address, port, reader);
            receiveSocket.Start();


            var sendSocket = new PgmSource(address, port);
            sendSocket.Start();
            var asyncWriter = new BatchWriter(sendSocket, 1500);
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