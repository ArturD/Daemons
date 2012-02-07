using System;
using System.Text.RegularExpressions;
using Daemons.MQ.Emcaster;
using Daemons.Util;

namespace Daemons.MQ.Integration.Emcaster
{
    public class EmRoute : IMqRoute
    {
        private readonly IMulticastingChannel _channel;
        private readonly Regex _pattern;
        private readonly CopyOnWriteList<Action<string, object>> _list =new CopyOnWriteList<Action<string,object>>();

        public EmRoute(string pattern, IMulticastingChannel channel)
        {
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (channel == null) throw new ArgumentNullException("channel");

            _channel = channel;
            _pattern = new Regex(pattern, RegexOptions.Compiled);
            _channel.Subscribe(pattern, (p, o) =>
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
            _channel.Publish(path, message);
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
            _channel.Dispose();
        }
    }
}