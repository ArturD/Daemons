using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NLog;

namespace Agents
{
    public class Process : IProcess
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IScheduler _scheduler;
        private readonly IMessageEndpoint _messageEndpoint;
        private readonly List<IMessageHandler> _handlers = new List<IMessageHandler>();
        private readonly List<Action> _shutdownActions = new List<Action>(); 

        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        public IMessageEndpoint MessageEndpoint
        {
            get { return _messageEndpoint; }
        }

        public Process(IScheduler scheduler)
        {
            if(Logger.IsTraceEnabled) Logger.Trace("Starting Process {0}", GetHashCode());
            _shutdownActions.Add(() => Logger.Trace("Shutdown Process {0}", GetHashCode()));
            _scheduler = scheduler;
            _messageEndpoint = new ProcessMessageEndpoint(
                message => _scheduler.Schedule(
                    () =>
                        {
                            if (Logger.IsTraceEnabled) Logger.Trace("Started Process {0}", GetHashCode());
                            foreach (var messageHandler in _handlers)
                            {
                                try
                                {
                                    if (messageHandler.TryHandle(message)) break;
                                }
                                catch (Exception ex)
                                {
                                    Logger.TraceException("Unexpected exception", ex);
                                    throw new TargetInvocationException(ex);
                                }
                            }
                        }));
        }

        public void OnMessage<TMessage>(Action<TMessage> action)
        {
            _handlers.Add(new MessageHandler<TMessage>(action));
        }

        public void OnShutdown(Action shutdownAction)
        {
            _shutdownActions.Add(shutdownAction);
        }

        public void Shutdown()
        {
            if(Logger.IsTraceEnabled) Logger.Trace("Scheduling shutdown at {0}", GetHashCode());
            _scheduler.Schedule(
                () =>
                    {
                        if (Logger.IsTraceEnabled) Logger.Trace("Running shutdown actions at {0}", GetHashCode());
                        foreach (var shutdownAction in _shutdownActions)
                        {
                            shutdownAction();
                        }
                        _scheduler.Dispose();
                    });
        }

        internal class MessageHandler<TMessage> : IMessageHandler
        {
            private readonly Action<TMessage> _action;

            public MessageHandler(Action<TMessage> action)
            {
                _action = action;
            }

            public bool TryHandle(MessageContext messageContext)
            {
                if (messageContext.Message is TMessage)
                {
                    _action((TMessage) messageContext.Message);
                    return true;
                }
                return false;
            }
        }

        internal class ProcessMessageEndpoint : IMessageEndpoint
        {
            private Action<MessageContext> _sendMessageAction;

            public ProcessMessageEndpoint(Action<MessageContext> sendMessageAction)
            {
                _sendMessageAction = sendMessageAction;
            }

            public void SendMessage(MessageContext context)
            {
                _sendMessageAction(context);
            }
        }
    }


    public interface IMessageEndpoint
    {
        void SendMessage(MessageContext context);
    }

    public interface IMessageHandler
    {
        bool TryHandle(MessageContext messageContext);
    }

    public class MessageContext
    {
        public string Id { get; set; }
        public Object Message { get; set; }
    }
}
