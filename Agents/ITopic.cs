namespace Agents
{
    public interface ITopic<TMessage> : IPublisher<TMessage>, ISubscriber<TMessage>
    {
    }
}
