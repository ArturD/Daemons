using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Agents.MessageBus;
using Agents.Util;
using NLog;

namespace Agents
{
    public class Process : IProcess
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IScheduler _scheduler;
        private readonly IMessageBus _messageBus;
        private readonly IMessageEndpoint _messageEndpoint;
        private readonly SortedSet<IMessageHandlerWithPriority> _handlers = new SortedSet<IMessageHandlerWithPriority>();
        private readonly List<Action> _shutdownActions = new List<Action>();

        public IMessageBus MessageBus
        {
            get { return _messageBus; }
        }

        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        public IMessageEndpoint MessageEndpoint
        {
            get { return _messageEndpoint; }
        }

        public Process(IScheduler scheduler, IMessageBusFactory messageBusFactory)
        {
            DateTime start = DateTime.UtcNow;
            if(Logger.IsTraceEnabled) Logger.Trace("Starting Process {0}", GetHashCode());
            _shutdownActions.Add(() => Logger.Trace("Shutdown Process {0} life-time {1}", GetHashCode(), (DateTime.UtcNow - start).TotalMilliseconds));
            _scheduler = scheduler;
            _messageBus = messageBusFactory.Create(this);
            _messageEndpoint = new ProcessMessageEndpoint(this);
        }

        public IDisposable OnMessage<TMessage>(Action<TMessage, IMessageContext> action)
        {
            return OnMessage(action, 0);
        }

        public IResponseContext SendTo(IProcess targetProcess, object message)
        {
            var responseContext = new ResponseMessageContext(this);
            targetProcess.MessageEndpoint.QueueMessage(message, responseContext);
            return responseContext;
        }

        public IDisposable OnMessage<TMessage>(Action<TMessage, IMessageContext> action, int priority)
        {
            var handler = new MessageHandler<TMessage>(action, priority);
            return AddMessageHandler(handler);
        }

        private IDisposable AddMessageHandler<TMessage>(MessageHandler<TMessage> handler)
        {
            _handlers.Add(handler);
            //_handlers.Sort((x, y) => x.Priority - y.Priority);
            return new AnonymousDisposer(() => _handlers.Remove(handler));
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

        internal interface IMessageHandlerWithPriority : IMessageHandler, IComparable<IMessageHandlerWithPriority>
        {
            int Priority { get; }
        }

        internal class MessageHandler<TMessage> : IMessageHandlerWithPriority
        {
            private readonly Func<TMessage, IMessageContext, bool> _handleFunction;

            public int Priority { get; protected set; }

            public MessageHandler(Func<TMessage, IMessageContext, bool> handleFunction, int priority)
            {
                _handleFunction = handleFunction;
                Priority = priority;
            }

            public MessageHandler(Action<TMessage, IMessageContext> handleAction, int priority)
                : this((m, c) =>
                {
                    handleAction(m, c);
                    return true;
                }, priority)
            {
            }

            public bool TryHandle(object message, IMessageContext context)
            {
                if (message is TMessage)
                {
                    return  _handleFunction((TMessage) message, context);
                }
                return false;
            }

            public int CompareTo(IMessageHandlerWithPriority other)
            {
                var diff = Math.Sign(Priority - other.Priority);
                if (diff != 0)
                    return diff;
                return GetHashCode() - other.GetHashCode(); // FIXME
            }
        }

        internal class ProcessMessageEndpoint : IMessageEndpoint
        {
            private readonly Process _process;

            public ProcessMessageEndpoint(Process process)
            {
                _process = process;
            }

            public void QueueMessage(object message, IMessageContext context)
            {
                _process._scheduler.Schedule(
                    () =>
                        {
                            if (Logger.IsTraceEnabled) Logger.Trace("Started Process {0}", GetHashCode());
                            foreach (var messageHandler in _process._handlers)
                            {
                                try
                                {
                                    if (messageHandler.TryHandle(message, context)) break;
                                }
                                catch (Exception ex)
                                {
                                    Logger.TraceException("Unexpected exception", ex);
                                    throw new TargetInvocationException(ex);
                                }
                            }
                        });
            }
        }

        public class ResponseMessageContext : IMessageContext, IResponseContext
        {
            private readonly Process _hostProcess;
            private State _state = State.Waiting;
          
            public IProcess HostProcess
            {
                get { return _hostProcess; }
            }

            public ResponseMessageContext(Process hostProcess)
            {
                _hostProcess = hostProcess;
            }

            public void Response(object message)
            {
                _hostProcess.MessageEndpoint.QueueMessage(message, new ZeroResponseContext(this));
            }

            public IResponseContext ExpectMessage(Action<object, IMessageContext> consume)
            {
                IDisposable disposer = null;
                disposer = _hostProcess.AddMessageHandler( new MessageHandler<object>(
                    (o, c) =>
                        {
                            var context = c as ZeroResponseContext;
                            if (context == null || context.OriginalMessageContext != this)
                                return false;

                            consume(o, c);
                            _state = State.Executed;
                            // disposer should be propper at this point if ExpectMessage is called from correct process
                            Debug.Assert(disposer != null, "disposer != null");
                            //_hostProcess.Scheduler.Schedule(disposer.Dispose);
                            disposer.Dispose();
                            return true;
                        }, -10));
                return this;
            }

            public IResponseContext ExpectMessage<T>(Action<T, IMessageContext> consume)
            {
                return ExpectMessage((o, e) => consume((T) o, e));
            }

            public IResponseContext ExpectTimeout(TimeSpan timeout, Action timeoutAction)
            {
                _hostProcess.Scheduler.ScheduleOne(
                    () =>
                        {
                            if(_state == State.Waiting)
                                timeoutAction();
                        }, timeout);
                return this;
            }

            public IResponseContext ExpectError(object error)
            {
                throw new NotImplementedException();
            }
            private enum State
            {
                Waiting,
                Executed,
                TimedOut,
                Error,
            }

            class ZeroResponseContext : IMessageContext
            {
                private readonly ResponseMessageContext _originalMessageContext;

                public ResponseMessageContext OriginalMessageContext
                {
                    get { return _originalMessageContext; }
                }

                public ZeroResponseContext(ResponseMessageContext originalMessageContext)
                {
                    _originalMessageContext = originalMessageContext;
                }

                public void Response(object message)
                {
                    throw new NotSupportedException("Response not supported for this message.");
                }
            }
        }
    }
}
