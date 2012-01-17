using System;
using Agents.Util;

namespace Agents
{
    public class Topic<TMessage> : ITopic<TMessage>
    {
        private readonly CopyOnWriteList<Subscribtion> _subscribtions = new CopyOnWriteList<Subscribtion>();

        public void Publish(TMessage message)
        {
            foreach (var subscribtion in _subscribtions)
            {
                subscribtion.OnMessage(message);
            }
        }

        public IDisposable Subscribe(Action<TMessage> consumer)
        {
            return Subscribe(Daemons.CurrentScheduler, consumer);
        }

        public IDisposable Subscribe(IScheduler scheduler, Action<TMessage> consumeAction)
        {
            var subscribtion = new Subscribtion(this, scheduler, consumeAction);
            _subscribtions.Add(subscribtion);
            return subscribtion;
        }

        public class Subscribtion : ISubscribtion<TMessage>, IDisposable
        {
            private readonly Topic<TMessage> _topic;
            private readonly IScheduler _scheduler;
            private readonly Action<TMessage> _consumerAction;

            public Subscribtion(Topic<TMessage> topic, IScheduler scheduler, Action<TMessage> consumerAction)
            {
                _topic = topic;
                _scheduler = scheduler;
                _consumerAction = consumerAction;
            }

            public void OnMessage(TMessage message)
            {
                _scheduler.Schedule(()=> _consumerAction(message));
            }

            /// <summary>
            /// Remove object from list of subscribtions;
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                _topic._subscribtions.Remove(this);
            }
        } 
    }
}