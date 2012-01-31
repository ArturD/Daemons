namespace Daemons
{
    public interface ITopic<TMessage> : IPublisher<TMessage>, ISubscriber<TMessage>
    {
    }
}
