using System;
using System.Text.RegularExpressions;
using Daemons.Util;

namespace Daemons.MQ.Emcaster
{
    public class CompositeSubscriber
    {
        private readonly CopyOnWriteList<Action<string,object>> _subscribtions 
            = new CopyOnWriteList<Action<string, object>>();

        public void Trigger(string path, object message)
        {
            foreach (var subscribtion in _subscribtions)
            {
                subscribtion(path, message);
            }
        }

        public IDisposable Subscribe(string topicPattern, Action<string, object> messageConsumer)
        {
            var topicExpression = new Regex(topicPattern);
            Action<string, object> conditionalConsume = 
                (path, message) =>
                    {
                        if (topicExpression.IsMatch(path))
                            messageConsumer(path, message);
                    };
            _subscribtions.Add(conditionalConsume);
            return new AnonymousDisposer(() => _subscribtions.Remove(conditionalConsume));
        }
    }
}