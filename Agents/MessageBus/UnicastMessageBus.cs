using System;
using System.Collections.Concurrent;

namespace Agents.MessageBus
{
    public class UnicastMessageBus : IMessageBus
    {
        private readonly IProcess _hostProcess;
        private readonly ConcurrentDictionary<string, IProcess> _concurrentDictionary;

        public UnicastMessageBus(IProcess hostProcess, ConcurrentDictionary<string, IProcess> concurrentDictionary)
        {
            _hostProcess = hostProcess;
            _concurrentDictionary = concurrentDictionary;
        }

        public IResponseContext Publish(string path, object message)
        {
            IProcess process;
            if (_concurrentDictionary.TryGetValue(path, out process))
            {
                return _hostProcess.SendTo(process, message, path);
            }
            throw new NotImplementedException(); // TODO
        }

        public void Subscribe<TMessage>(string path, Action<TMessage, IMessageContext> consumer)
        {
            Subscribe(path, _hostProcess);
            _hostProcess.OnMessage(path, consumer, -10);
        }

        public void Subscribe(string path, IProcess process)
        {
            if (!_concurrentDictionary.TryAdd(path, process))
                throw new InvalidOperationException(
                    "Some process already subscribed to this path. UnicastMessageBus handles only unicast subscribe.");
        }
    }
}