using Emcaster.Sockets;
using Emcaster.Topics;

namespace Daemons.MQ.Integration.Emcaster
{
    public class PgmSubscriber : EmSubscriberBase, IEmSubscriber
    {
        readonly MessageParserFactory _factory;
        private readonly PgmReceiver _receiveSocket;

        public PgmSubscriber(string address, int port)
        {
            _factory = new MessageParserFactory();
            var reader = new PgmReader(_factory);
            _receiveSocket = new PgmReceiver(address, port, reader);
            _receiveSocket.Start();
        }

        protected override IMessageEvent MessageParserFactory
        {
            get { return _factory; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _receiveSocket.Dispose();
        }
    }
}