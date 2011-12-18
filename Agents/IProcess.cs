using System;

namespace Agents
{
    public interface IProcess
    {
        IScheduler Scheduler { get; }
        IMessageEndpoint MessageEndpoint { get; }
        void OnMessage<TMessage>(Action<TMessage> action);
        void OnShutdown(Action shutdownAction);
        void Shutdown();
    }
}