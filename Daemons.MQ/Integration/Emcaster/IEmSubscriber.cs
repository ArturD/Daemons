using System;

namespace Daemons.MQ.Integration.Emcaster
{
    public interface IEmSubscriber : IDisposable
    {
        IDisposable Subscribe(string pattern, Action<string, object> consume);
    }
}