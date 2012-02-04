using System;

namespace Daemons.MessageBus
{
    public interface IMessageBus
    {
        void Publish<T>(string path, T message);
        IDisposable Subscribe<T>(string path, Action<T> consume);
    }
}
