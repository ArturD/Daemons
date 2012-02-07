using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons.MQ.Emcaster
{
    public interface IMulticastingChannel : IDisposable
    {
        void Publish(string path, object message);
        IDisposable Subscribe(string topicPattern, Action<string, object> messageConsumer);
    }
}
