using System;

namespace Daemons
{
    public interface IFutureEvent
    {
        bool Done { get; }
        void Trigger();
        void RegisterHandler(Action continuation);
    }
}