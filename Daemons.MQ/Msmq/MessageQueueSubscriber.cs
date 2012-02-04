using System;
using System.Messaging;
using System.Threading;
using Daemons.Util;

namespace Daemons.MQ.Msmq
{
    public class MessageQueueSubscriber : IDisposable
    {
        private readonly MessageQueue _messageQueue;
        private readonly CopyOnWriteList<Action<object>> _subscribers = new CopyOnWriteList<Action<object>>();
        // 0 - before subscribe, 1 - after some subscribtions, 2 - after disposal
        private int _state = 0;

        public MessageQueueSubscriber(MessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public IDisposable Subscribe(Action<object> consumer)
        {
            _subscribers.Add(consumer);
            EnsureWatching();
            return new AnonymousDisposer(() => _subscribers.Remove(consumer));
        }

        private void EnsureWatching()
        {
            if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
            {
                _messageQueue.ReceiveCompleted += OnReceive;
                ReceiveAsyncLoop();
            }
        }

        private void OnReceive(object sender, ReceiveCompletedEventArgs e)
        {
            Message message = _messageQueue.EndReceive(e.AsyncResult);
            foreach (var subscriber in _subscribers)
            {
                subscriber.Invoke(message.Body);
            }
            ReceiveAsyncLoop();
        }

        private void ReceiveAsyncLoop()
        {
            if (_state != 1) return;
            _messageQueue.BeginReceive();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, 2) == 1)
            {
                _messageQueue.Dispose();
                _subscribers.Clear();
            }
        }
    }
}