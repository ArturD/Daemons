using System;
using Agents.MessageBus;

namespace Agents
{
    public interface IProcess
    {
        IScheduler Scheduler { get; }
        IMessageEndpoint MessageEndpoint { get; }
        IMessageBus MessageBus { get; }
        IDisposable OnMessage<TMessage>(Action<TMessage, IMessageContext> action);
        IDisposable OnMessage<TMessage>(Action<TMessage, IMessageContext> action, int priority);
        IResponseContext SendTo(IProcess targetProcess, object message);
        void OnShutdown(Action shutdownAction);
        void Shutdown();
    }
}