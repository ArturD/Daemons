namespace Agents
{
    public interface IMessageContext
    {
        string Path { get; }
        void Response(object message);
    }
}