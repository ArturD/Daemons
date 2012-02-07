using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons.MQ.Emcaster.Reliability
{
    public class OutputMovingWindow : IDisposable
    {
        private readonly Dictionary<int, OutputMessage> _messagesDictionary = new Dictionary<int, OutputMessage>();
        private readonly LinkedList<OutputMessage> _messages = new LinkedList<OutputMessage>();
        private readonly ThreadPoolDaemon _daemon = new ThreadPoolDaemon();
        private readonly TimeSpan _messageLifetime;
        private readonly IDisposable _interval;

        public OutputMovingWindow(TimeSpan messageLifetime)
        {
            _messageLifetime = messageLifetime;
            _interval = _daemon.ScheduleInterval(ClearOldMessages, messageLifetime);
        }

        public OutputMovingWindow()
            : this(TimeSpan.FromSeconds(60))
        {
        }

        public void Add(string path, MessageBase message)
        {
            var outputMessage = new OutputMessage(path, message);
            lock (this)
            {
                _messagesDictionary.Add(outputMessage.Message.MessageNo, outputMessage);
                _messages.AddLast(outputMessage);
            }
        }

        /// <summary>
        /// Returns messsage if it's still in buffer, Null otherwise.
        /// </summary>
        /// <param name="messageNo"></param>
        /// <returns></returns>
        public OutputMessage Find(int messageNo)
        {
            lock (this)
            {
                OutputMessage message;
                if (_messagesDictionary.TryGetValue(messageNo, out message))
                {
                    return message;
                }
                return null;
            }
        }

        private void ClearOldMessages()
        {
            lock (this)
            {
                var currentNode = _messages.First;
                while (currentNode != null && (currentNode.Value.PublishedAt < DateTime.UtcNow - _messageLifetime))
                {
                    var next = currentNode.Next;
                    _messages.Remove(currentNode);
                    _messagesDictionary.Remove(currentNode.Value.Message.MessageNo);
                    currentNode = next;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _interval.Dispose();
            _daemon.Dispose();
        }

        public class OutputMessage
        {
            public OutputMessage(string path, MessageBase message)
                : this(path, message, DateTime.UtcNow)
            {
            }

            public OutputMessage(string path, MessageBase message, DateTime publishedAt)
            {
                Path = path;
                Message = message;
                PublishedAt = publishedAt;
            }

            public string Path { get; set; }
            public MessageBase Message { get; set; }
            public DateTime PublishedAt { get; set; }
        }
    }

}
