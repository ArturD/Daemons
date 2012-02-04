using Emcaster.Sockets;
using Emcaster.Topics;

namespace Daemons.MQ.Integration.Emcaster
{
    public class PgmPublisherFactory : IEmPublisherFactory
    {
        private readonly BatchWriter _asyncWriter;
        public PgmPublisherFactory(string address, int port)
        {
            var sendSocket = new PgmSource(address, port);
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