using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents.MessageBus
{
    public interface IMessageBus
    {
        IResponseContext Publish(string path, object message);
        void Subscribe<TMessage>(string path, Action<TMessage, IMessageContext> consumer);
    }
}
