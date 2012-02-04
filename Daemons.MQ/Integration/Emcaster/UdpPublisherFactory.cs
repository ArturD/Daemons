using Emcaster.Sockets;
using Emcaster.Topics;

namespace Daemons.MQ.Integration.Emcaster
{
    public class UdpPublisherFactory : IEmPublisherFactory
    {
        private readonly BatchWriter _asyncWriter;
        public UdpPublisherFactory(string address, int port)
        {
            var sendSocket = new UdpSource(address, port);
            sendSocket.Start();
            _asyncWriter = new BatchWriter(sendSocket, 1500);
        }

        public TopicPublisher CreatePublisher()
        {
            var topicPublisher = new TopicPublisher(_asyncWriter);
            topicPublisher.Start();

            return topicPublisher;
        }
    }
}
