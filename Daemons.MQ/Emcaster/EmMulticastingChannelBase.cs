using System;
using Common.Logging;
using Emcaster.Topics;

namespace Daemons.MQ.Emcaster
{
    public abstract class EmMulticastingChannelBase: IMulticastingChannel
    {
        private readonly ILog Logger = LogManager.GetCurrentClassLogger();
        protected MessageParserFactory MessageParserFactory;
        protected TopicPublisher TopicPublisher;

        public void Publish(string path, object message)
        {
            Logger.DebugFormat("publishing {1} on {0}.", path, message);
            TopicPublisher.PublishObject(path, message, 60000); // TODO: fix this timeout thing
        }

        public IDisposable Subscribe(string topicPattern, Action<string, object> messageConsumer)
        {
            var topicSubscriber = new TopicSubscriber(topicPattern, MessageParserFactory);
            topicSubscriber.TopicMessageEvent += messageParser =>
                                                     {
                                                         object o = messageParser.ParseObject();
                                                         messageConsumer(messageParser.Topic, o);
                                                     };
            topicSubscriber.Start();

            return topicSubscriber;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public abstract void Dispose();
    }
}