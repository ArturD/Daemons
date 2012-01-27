using System;

namespace Agents.MessageBus
{
    public interface IMessageBus
    {
        ITopic<T> Topic<T>(string path);
        void Publish<T>(string path, T message);
        IDisposable Subscribe<T>(string path, Action<T> consume);
    }
}
