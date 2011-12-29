using System;

namespace Agents
{
    public interface IResponseContext
    {
        IResponseContext ExpectMessage(Action<object, IMessageContext> consume);
        IResponseContext ExpectMessage<T>(Action<T, IMessageContext> consume);
        IResponseContext ExpectTimeout(TimeSpan timeout, Action timeoutAction);
        IResponseContext ExpectError(object error);
    }
}