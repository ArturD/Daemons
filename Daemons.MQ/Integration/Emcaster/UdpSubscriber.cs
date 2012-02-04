using Emcaster.Sockets;
using Emcaster.Topics;

namespace Daemons.MQ.Integration.Emcaster
{
    public class UdpSubscriber : EmSubscriberBase, IEmSubscriber
    {
        private readonly MessageParserFactory _factory;
        private readonly UdpReceiver _receiveSocket;
        public UdpSubscriber(string address, int port)
        {
            _factory = new MessageParserFactory();
            MessageParser parser = _factory.Create();
            _receiveSocket = new UdpReceiver(address, port);
            _receiveSocket.ReceiveEvent += parser.OnBytes;
            
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
