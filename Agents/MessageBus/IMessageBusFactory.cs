namespace Agents.MessageBus
{
    public interface IMessageBusFactory
    {
        IMessageBus Create(IProcess process);
    }
}