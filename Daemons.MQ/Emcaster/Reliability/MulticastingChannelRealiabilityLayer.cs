using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;

namespace Daemons.MQ.Emcaster.Reliability
{
    public class MulticastingChannelRealiabilityLayer : IMulticastingChannel, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private readonly string _connectionId = Guid.NewGuid().ToString();
        private readonly IMulticastingChannel _innerChannel;
        private readonly ConcurrentDictionary<string, InputMovingWindow> _windows =new ConcurrentDictionary<string, InputMovingWindow>();
        private readonly IDisposable _subscribtion;
        private readonly CompositeSubscriber _subscriber = new CompositeSubscriber();
        private readonly OutputMovingWindow _outputWindow = new OutputMovingWindow();
        private readonly ThreadPoolDaemon _daemon = new ThreadPoolDaemon();
        private readonly TimeSpan _heartbeatInterval;
        private readonly IDisposable _heartbeat;
        private readonly ISet<InputMovingWindow> _regeneratingWindows = new HashSet<InputMovingWindow>();
        private int _lastSentMessage = 0;


        public MulticastingChannelRealiabilityLayer(IMulticastingChannel innerChannel, TimeSpan heartbeatInterval)
        {
            _innerChannel = innerChannel;
            _heartbeatInterval = heartbeatInterval;
            _subscribtion = _innerChannel.Subscribe(".*", OnLowerLayerMessage);

            _heartbeat = _daemon.ScheduleInterval(Heartbeat, _heartbeatInterval);
        }

        public void Publish(string path, object message)
        {
            var messageNo = Interlocked.Increment(ref _lastSentMessage);
            var payload = new Message
                              {
                                  ConnectionId = _connectionId,
                                  MessageNo = messageNo,
                                  InnerObject = message,
                              };
            _outputWindow.Add(path, payload);

            _daemon.Schedule(() => _innerChannel.Publish(path, payload));
            // retry after 300 ms
           // _daemon.ScheduleOne(() => _innerChannel.Publish(path, payload), TimeSpan.FromMilliseconds(300));
        }

        public IDisposable Subscribe(string topicPattern, Action<string, object> messageConsumer)
        {
            return _subscriber.Subscribe(topicPattern, messageConsumer);
        }

        protected void OnLowerLayerMessage(string path, object messageObject)
        {
            // _subscriber.Trigger(path, message);
            if (messageObject is MessageBase)
            {
                var messageBase = (MessageBase) messageObject;

                OnLowerLayerMessagePayload(path, messageBase);
            }
            else if (messageObject is HeartBeatMessage)
            {
                var heartbeat = (HeartBeatMessage) messageObject;
                OnLowerLayerHeartbeatMessage(heartbeat);
            }
            else if (messageObject is RegenerateRequest)
            {
                var regenerationRequest = (RegenerateRequest) messageObject;
                OnLowerLayerRegenerateRequestMessage(regenerationRequest);
            }
        }

        private void OnLowerLayerRegenerateRequestMessage(RegenerateRequest regenerationRequest)
        {
            if (_connectionId != regenerationRequest.NodeId) return;
            var foundMessage = _outputWindow.Find(regenerationRequest.MissingMessageNo);
            if (foundMessage == null) return;
            _innerChannel.Publish(foundMessage.Path, foundMessage.Message);
        }

        private void OnLowerLayerHeartbeatMessage(HeartBeatMessage heartbeat)
        {
            var window = _windows.GetOrAdd(heartbeat.ConnectionId, _ => new InputMovingWindow());
            window.UpdateLastSentMessage(heartbeat.LastSentMessage);
            AskForRegeneration(heartbeat.ConnectionId, window);
        }

        private void OnLowerLayerMessagePayload(string path, MessageBase messageBase)
        {
            var window = _windows.GetOrAdd(messageBase.ConnectionId, _ => new InputMovingWindow());
            window.AddMessage(path, messageBase);
            var flushedMessages = window.Flush();
            foreach (var flushed in flushedMessages)
            {
                if (flushed.Message is Message)
                {
                    _subscriber.Trigger(flushed.Path, ((Message) flushed.Message).InnerObject);
                }
            }

            AskForRegeneration(messageBase.ConnectionId, window);
        }

        private void AskForRegeneration(string nodeId, InputMovingWindow window)
        {
            lock (_regeneratingWindows) if(!_regeneratingWindows.Add(window)) return;
            AskForRegenerationRepetition(nodeId, window);
        }

        private void AskForRegenerationRepetition(string nodeId, InputMovingWindow window)
        {
            try
            {
                IEnumerable<int> missingMessages = window.GetMissingMessages();
                bool any = false;
                foreach (var missingMessageNo in missingMessages)
                {
                    any = true;
                    _innerChannel.Publish("/emcaster/reliability/regenerateReq",
                                          new RegenerateRequest(nodeId, missingMessageNo));
                }
                if (any)
                    _daemon.ScheduleOne(() => AskForRegenerationRepetition(nodeId, window),
                                        TimeSpan.FromMilliseconds(200));
                else
                    lock (_regeneratingWindows) _regeneratingWindows.Remove(window);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error.", ex);
                throw ex;
            }
        }

        private void Heartbeat()
        {
            _innerChannel.Publish(@"/emcaster/reliability/heartbeat", new HeartBeatMessage(_connectionId, _lastSentMessage));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _subscribtion.Dispose();
            //_subscriber.Dispose();
            _heartbeat.Dispose();
            _daemon.Dispose();
            _outputWindow.Dispose();
        }
    }
}