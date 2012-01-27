using System;
using System.Collections.Concurrent;

namespace Agents.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<string, object> _topics
            = new ConcurrentDictionary<string, object>();
  
        public ITopic<T> Topic<T>(string path)
        {
            var topic = _topics.GetOrAdd(path, p => new Topic<T>());
            if (topic is ITopic<T>)
            {
                return topic as ITopic<T>;
            }
            throw new InvalidOperationException("Topic has been used with diffrent type.");
        }
    }
}