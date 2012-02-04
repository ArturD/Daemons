using System;
using System.Text.RegularExpressions;
using Daemons.Util;

namespace Daemons.MQ.Integration.Emcaster
{
    public class EmRoute : IMqRoute
    {
        private readonly IEmPublisherFactory _publisherFactory;
        private readonly IEmSubscriber _subscriber;
        private readonly Regex _pattern;
        private readonly CopyOnWriteList<Action<string, object>> _list =new CopyOnWriteList<Action<string,object>>();

        public EmRoute(IEmPublisherFactory publisherFactory, IEmSubscriber subscriber, string pattern)
        {
            if (publisherFactory == null) throw new ArgumentNullException("publisherFactory");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            if (pattern == null) throw new ArgumentNullException("pattern");

            _publisherFactory = publisherFactory;
            _subscriber = subscriber;
            _pattern = new Regex(pattern, RegexOptions.Compiled);
            subscriber.Subscribe(pattern, (p, o) =>
                                              {
                                                  foreach (Action<string,object> consumer in _list)
                                                  {
                                                      consumer(p, o);
                                                  }
                                              });
        }

        public bool CanPublish<T>(string path, T message)
        {
            return _pattern.IsMatch(path);
        }

        public void Publish<T>(string path, T message)
        {
            _publisherFactory.CreatePublisher().PublishObject(path, message, 30000);
        }

        public bool CanSubscribe<T>(string path)
        {
            return _pattern.IsMatch(path);
        }

        public IDisposable Subscribe<T>(string path, Action<T> consumeAction)
        {
            Action<string, object> consumer = (pattern, message) =>
                               {
                                   if (path == pattern && message is T) consumeAction((T) message);
                               };
            _list.Add(consumer);
            return new AnonymousDisposer(() => _list.Remove(consumer));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _subscriber.Dispose();
        }
    }
}