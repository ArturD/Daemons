using System;

namespace Daemons.MQ.Msmq
{
    // TODO: think :P! It's same as IMessageBus, will using IMessageBus be confusing ?

    /// <summary>
    /// 
    /// </summary>
    public interface IMsmqService 
    {
        void Publish<TMessage>(string path, TMessage message);
        IDisposable Subscribe<TMessage>(string path, Action<TMessage> consumeAction);
    }
}