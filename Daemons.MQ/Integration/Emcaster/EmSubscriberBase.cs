using System;
using Emcaster.Topics;

namespace Daemons.MQ.Integration.Emcaster
{
    public abstract class EmSubscriberBase
    {
        public virtual IDisposable Subscribe(string pattern, Action<string, object> consume)
        {
            var topicSubscriber = new TopicSubscriber(pattern, MessageParserFactory);
            topicSubscriber.TopicMessageEvent += messageParser =>
                                                     {
                                                         object o = messageParser.ParseObject();
                                                         consume(messageParser.Topic, o);
                                                     };
            topicSubscriber.Start();

            return topicSubscriber;
        }

        protected abstract IMessageEvent MessageParserFactory { get; }
    }
}