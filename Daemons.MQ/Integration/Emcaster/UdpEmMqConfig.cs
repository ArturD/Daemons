using System.Collections.Generic;

namespace Daemons.MQ.Integration.Emcaster
{
    public class UdpEmMqConfig : MqConfig
    {
        private readonly UdpPublisherFactory _publisherFactory;
        private readonly UdpSubscriber _subscriber;

        public UdpEmMqConfig(string address, int port) : base(new List<IMqRoute>())
        {
            _publisherFactory = new UdpPublisherFactory(address ,port);
            _subscriber = new UdpSubscriber(address, port);
        }

        public UdpEmMqConfig AddRoute(string pattern)
        {
            AddRoute(new EmRoute(_publisherFactory, _subscriber, pattern));
            return this;
        }
    }
}
