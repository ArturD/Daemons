using System;
using Daemons.MessageBus;

namespace Daemons.MQ
{
    public class LocalMessageBusRoute : IMqRoute
    {
        private readonly IMessageBus _messageBus;

        public LocalMessageBusRoute() : this(new SimpleMessageBus())
        {
        }

        public LocalMessageBusRoute(IMessageBus messageBus)
        {
            if (messageBus == null) throw new ArgumentNullException("messageBus");
            _messageBus = messageBus;
        }

        public bool CanPublish<T>(string path, T message)
        {
            return true;
        }

        public void Publish<T>(string path, T message)
        {
            _messageBus.Publish(path, message);
        }

        public bool CanSubscribe<T>(string path)
        {
            return true;
        }

        public IDisposable Subscribe<T>(string path, Action<T> consumeAction)
        {
            return _messageBus.Subscribe(path, consumeAction);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // _messageBus.Dispose();
        }
    }
}