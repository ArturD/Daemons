using System;
using System.Collections.Generic;
using Agents.MessageBus;
using Agents.Util;
using NLog;

namespace Agents
{
    public class Process : IProcess
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IScheduler _dispatcher;
        private readonly IMessageBus _messageBus;
        private readonly IMessageEndpoint _messageEndpoint;
        private readonly CompositeHandler _compositeHandler = new CompositeHandler();
        private readonly List<Action> _shutdownActions = new List<Action>();

        public IMessageBus MessageBus
        {
            get { return _messageBus; }
        }

        public IScheduler Dispatcher
        {
            get { return _dispatcher; }
        }

        public IMessageEndpoint MessageEndpoint
        {
            get { return _messageEndpoint; }
        }

        public Process(IScheduler scheduler, IMessageBusFactory messageBusFactory)
        {
            DateTime start = DateTime.UtcNow;
            if (Logger.IsTraceEnabled) Logger.Trace("Starting Process {0}", GetHashCode());
            _shutdownActions.Add(
                () =>
                Logger.Trace("Shutdown Process {0} life-time {1}", GetHashCode(),
                             (DateTime.UtcNow - start).TotalMilliseconds));
            _dispatcher = new DefaultSchedulerDispatcher(scheduler, this);
            _messageBus = messageBusFactory.Create(this);
            _messageEndpoint = new ProcessMessageEndpoint(this);
        }

        public IDisposable OnMessage<TMessage>(string path, Action<TMessage, IMessageContext> action)
        {
            return OnMessage(path, action, 0);
        }

        public IResponseContext SendTo(IProcess targetProcess, object message, string path)
        {
            var responseContext = new ResponseMessageContext(this, path);
            targetProcess.MessageEndpoint.QueueMessage(message, responseContext);
            return responseContext;
        }

        public IDisposable OnMessage<TMessage>(string path, Action<TMessage, IMessageContext> action, int priority)
        {
            var handler = new MessageHandler<TMessage>(action, path, priority);
            return AddMessageHandler(handler);
        }

        private IDisposable AddMessageHandler<TMessage>(MessageHandler<TMessage> handler)
        {
            return _compositeHandler.AddHandler(handler);
        }

        public void OnShutdown(Action shutdownAction)
        {
            _shutdownActions.Add(shutdownAction);
        }

        public void Shutdown()
        {
            if (Logger.IsTraceEnabled) Logger.Trace("Scheduling shutdown at {0}", GetHashCode());
            _dispatcher.Schedule(
                () =>
                    {
                        if (Logger.IsTraceEnabled) Logger.Trace("Running shutdown actions at {0}", GetHashCode());
                        foreach (var shutdownAction in _shutdownActions)
                        {
                            shutdownAction();
                        }
                        _dispatcher.Dispose();
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
            public string Path { get; set; }

            public MessageHandler(Func<TMessage, IMessageContext, bool> handleFunction, string path, int priority)
            {
                _handleFunction = handleFunction;
                Priority = priority;
                Path = path;
            }

            public MessageHandler(Action<TMessage, IMessageContext> handleAction, string path, int priority)
                : this((m, c) =>
                           {
                               handleAction(m, c);
                               return true;
                           }, path, priority)
            {
            }

            public bool TryHandle(object message, IMessageContext context)
            {
                if (message is TMessage)
                {
                    return _handleFunction((TMessage) message, context);
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
            internal readonly Process Process;

            public ProcessMessageEndpoint(Process process)
            {
                Process = process;
            }

            public void QueueMessage(object message, IMessageContext context)
            {
                var messageAndContext = new MessageAndContext(this, message, context);
                Process._dispatcher.Schedule(messageAndContext.Handle);
            }
        }

        private class MessageAndContext
        {
            public ProcessMessageEndpoint Endpoint { get; set; }
            public object Message { get; set; }
            public IMessageContext Context { get; set; }

            public MessageAndContext(ProcessMessageEndpoint endpoint, object message, IMessageContext context)
            {
                Endpoint = endpoint;
                Message = message;
                Context = context;
            }

            public void Handle()
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Started Process {0}", GetHashCode());
                Endpoint.Process._compositeHandler.TryHandle(Message, Context);

            }
        }

        public class ResponseMessageContext : IMessageContext, IResponseContext
        {
            private readonly Process _hostProcess;
            private State _state = State.Waiting;
            private Action<object, IMessageContext> _consume;

            public IProcess HostProcess
            {
                get { return _hostProcess; }
            }

            public ResponseMessageContext(Process hostProcess, string path)
            {
                _hostProcess = hostProcess;
                Path = path;
            }

            public string Path { get; private set; }

            public void Response(object message)
            {
                _hostProcess.MessageEndpoint.QueueMessage(message, new ZeroResponseContext(this) {Path = "/response"});
            }

            public IResponseContext ExpectResponse(Action<object, IMessageContext> consume)
            {
                _consume = consume;
                _hostProcess._compositeHandler.AddResponseHandler(this);
                return this;
            }

            public void Consume(object message, IMessageContext context)
            {
                if(_state == State.Waiting)
                {
                    _consume(message, context);
                    _state = State.Executed;
                }
            }

            public IResponseContext ExpectResponse<T>(Action<T, IMessageContext> consume)
            {
                return ExpectResponse((o, e) => consume((T) o, e));
            }

            public IResponseContext ExpectTimeout(TimeSpan timeout, Action timeoutAction)
            {
                _hostProcess.Dispatcher.ScheduleOne(
                    () =>
                        {
                            if (_state == State.Waiting)
                            {
                                _state = State.TimedOut;
                                timeoutAction();
                            }
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
        }

        private class ZeroResponseContext : IMessageContext
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

            public string Path { get; set; }

            public void Response(object message)
            {
                throw new NotSupportedException("Response not supported for this message.");
            }
        }

        private class CompositeHandler : IMessageHandler
        {
            private readonly HashSet<ResponseMessageContext> _responseContexts = new HashSet<ResponseMessageContext>();
            private readonly Dictionary<string, List<IMessageHandler>> _handlers = new Dictionary<string, List<IMessageHandler>>(); 

            public bool TryHandle(object message, IMessageContext context)
            {
                var zeroResponseContext =  context as ZeroResponseContext;
                if (zeroResponseContext != null)
                    if(TryHandleResponse(message, zeroResponseContext)) return true;

                List<IMessageHandler> handlers;
                if (_handlers.TryGetValue(context.Path, out handlers))
                {
                    foreach (var messageHandler in handlers)
                    {
                        if (messageHandler.TryHandle(message, context)) return true;
                    }
                }
                return false;
            }

            public IDisposable AddHandler<T>(MessageHandler<T> handler)
            {
                List<IMessageHandler> handlers;
                if (!_handlers.TryGetValue(handler.Path, out handlers))
                {
                    handlers = new List<IMessageHandler>();
                    _handlers.Add(handler.Path, handlers);
                }
                handlers.Add(handler);
                return new AnonymousDisposer(() => handlers.Remove(handler));
            }

            public IDisposable AddResponseHandler(ResponseMessageContext orignalMessageContext)
            {
                _responseContexts.Add(orignalMessageContext);
                return new AnonymousDisposer(() => _responseContexts.Remove(orignalMessageContext));
            }

            private bool TryHandleResponse(object message, ZeroResponseContext zeroResponseContext)
            {
                if (_responseContexts.Contains(zeroResponseContext.OriginalMessageContext))
                {
                    zeroResponseContext.OriginalMessageContext.Consume(message, zeroResponseContext);
                    return true;
                }
                return false;
            }
        }
    }
}
