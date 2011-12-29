namespace Agents
{
    public interface IMessageHandler
    {
        bool TryHandle(object message, IMessageContext context);
    }
}