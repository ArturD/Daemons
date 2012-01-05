using System;

namespace Agents
{
    public interface IResponseContext
    {
        IResponseContext ExpectResponse(Action<object, IMessageContext> consume);
        IResponseContext ExpectResponse<T>(Action<T, IMessageContext> consume);
        IResponseContext ExpectTimeout(TimeSpan timeout, Action timeoutAction);
        IResponseContext ExpectError(object error);
    }
}