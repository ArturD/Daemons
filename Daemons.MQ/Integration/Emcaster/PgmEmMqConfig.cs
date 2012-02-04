using System.Collections.Generic;

namespace Daemons.MQ.Integration.Emcaster
{
    public class PgmEmMqConfig : MqConfig
    {
        private readonly PgmPublisherFactory _publisherFactory;
        private readonly PgmSubscriber _subscriber;

        public PgmEmMqConfig(string address, int port)
            : base(new List<IMqRoute>())
        {
            _publisherFactory = new PgmPublisherFactory(address, port);
            _subscriber = new PgmSubscriber(address, port);
        }

        public PgmEmMqConfig AddRoute(string pattern)
        {
            AddRoute(new EmRoute(_publisherFactory, _subscriber, pattern));
            return this;
        }
    }
}