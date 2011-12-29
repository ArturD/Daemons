using Agents.MessageBus;

namespace Agents
{
    public interface IMessageEndpoint
    {
        void QueueMessage(object message, IMessageContext messageContext);
    }
}