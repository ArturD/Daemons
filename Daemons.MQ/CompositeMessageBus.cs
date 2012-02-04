using System;
using System.Collections.Generic;
using System.Linq;
using Daemons.MQ.Msmq;
using Daemons.MessageBus;

namespace Daemons.MQ
{
    public class CompositeMessageBus : IMessageBus
    {
        private readonly IList<IMqRoute> _routes;
        private IMqRoute _defaultRoute = new LocalMessageBusRoute();
 
        public CompositeMessageBus(IEnumerable<IMqConfig> messageQueues)
        {
            _routes = messageQueues
                .SelectMany(mq => mq.Routes())
                .ToList();
            _defaultRoute = new LocalMessageBusRoute();
        }

        public void Publish<T>(string path, T message)
        {
            var firstValidRoute = _routes.FirstOrDefault(route => route.CanPublish(path, message)) ?? _defaultRoute;
            if (firstValidRoute != null)
            {
                firstValidRoute.Publish(path, message);
            }
        }

        public IDisposable Subscribe<T>(string path, Action<T> consume)
        {
            var scheduler = Daemons.CurrentScheduler;
            var firstValidRoute = _routes.FirstOrDefault(route => route.CanSubscribe<T>(path)) ?? _defaultRoute;
            if (firstValidRoute != null)
            {
                return firstValidRoute.Subscribe(path, (T message) => scheduler.Schedule(() => consume(message)));
            }
            return new FakeDisposable();
        }
    }
}