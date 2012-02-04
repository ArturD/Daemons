using System;
using System.Collections.Generic;
using Daemons.MessageBus;

namespace Daemons.MQ
{
    public class CompositeMqFluentConfig : ICompositeMqFluentConfig
    {
        private readonly List<IMqConfig> _messageQueues = new List<IMqConfig>();
        private bool _frozen = false;
        private IMessageBus _messageBus = null;

        public ICompositeMqFluentConfig Add(IMqConfig config)
        {
            if(_frozen) throw new InvalidOperationException("Object is frozen. Cannot Add after Build."); 
            _messageQueues.Add(config);
            return this;
        }

        public IMessageBus Build()
        {
            _frozen = true;
            _messageBus = _messageBus ?? new CompositeMessageBus(_messageQueues);
            return _messageBus;
        }
    }
}