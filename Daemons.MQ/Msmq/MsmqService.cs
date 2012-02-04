using System;

namespace Daemons.MQ.Msmq
{
    public class MsmqService : IMsmqService
    {
        public void Publish<TMessage>(string path, TMessage message)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe<TMessage>(string path, Action<TMessage> consumeAction)
        {
            throw new NotImplementedException();
        }
    }
}