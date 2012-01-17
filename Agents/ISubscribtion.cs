namespace Agents
{
    public interface ISubscribtion<in T>
    {
        void OnMessage(T message);
    }
}