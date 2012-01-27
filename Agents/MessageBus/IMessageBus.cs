namespace Agents.MessageBus
{
    public interface IMessageBus
    {
        ITopic<T> Topic<T>(string path);
    }
}
