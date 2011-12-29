using System.Collections.Concurrent;

namespace Agents.MessageBus
{
    public class UnicastMessageBusFactory : IMessageBusFactory
    {
        private readonly ConcurrentDictionary<string, IProcess> _dictionary 
            = new ConcurrentDictionary<string, IProcess>();

        public IMessageBus Create(IProcess process)
        {
            return new UnicastMessageBus(process, _dictionary);
        }
    }
}
