using System;
using System.Messaging;
using Common.Logging;

namespace Daemons.MQ.Msmq
{
    public class MsmqQueue<T>
    {
// ReSharper disable StaticFieldInGenericType
        private static readonly ILog Logger = LogManager.GetLogger(typeof (MsmqQueue<T>));
// ReSharper restore StaticFieldInGenericType
        private readonly MessageQueue _messageQueue;
        private readonly MessageQueueSubscriber _subscriber;

        public MsmqQueue(string path) 
            : this(new MessageQueue(path, QueueAccessMode.SendAndReceive), new XmlMessageFormatter(new[] {typeof(T)}))
        {
        }

        public MsmqQueue(MessageQueue messageQueue) : this(messageQueue, null)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            _messageQueue = messageQueue;
        }

        public MsmqQueue(MessageQueue messageQueue, IMessageFormatter formatter)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            _messageQueue = messageQueue;
            _messageQueue.Formatter = formatter;
            _subscriber = new MessageQueueSubscriber(_messageQueue);
        }

        public void Publish(T message)
        {
            _messageQueue.Send(message);
        }

        public IDisposable Subscribe(Action<T> consumer)
        {
            var disposer = _subscriber.Subscribe(messageObject =>
                                      {
                                          if (messageObject is T)
                                          {
                                              consumer((T)messageObject);
                                          }
                                          else
                                          {
                                              Logger.WarnFormat("Unexpected message type {0}. {1} expected.", messageObject.GetType(), typeof(T));
                                          }
                                      });
            return disposer;
        }
    }
}
