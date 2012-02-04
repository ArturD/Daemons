using System;

namespace Daemons.MQ.Msmq
{
    public class StaticMsmqRoute : IMqRoute
    {
        private readonly string _pattern;

        public StaticMsmqRoute(IMsmqService service, string pattern)
        {
            _pattern = pattern;
        }

        public bool CanPublish<T>(string path, T message)
        {
            return MatchWithPattern(path);
        }

        public void Publish<T>(string path, T message)
        {
            // TODO
            //_service.Publish(path, message);
            throw new NotImplementedException();
        }

        public bool CanSubscribe<T>(string path)
        {
            return MatchWithPattern(path);
        }

        public IDisposable Subscribe<T>(string path, Action<T> consumeAction)
        {
            // TODO
            // return _service.Subscribe(path, consumeAction);
            throw new NotImplementedException();
        }

        protected virtual bool MatchWithPattern(string path)
        {
            return _pattern == path;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}