using System;

namespace Agents
{
    public interface ISynchedFuture<out T>
    {
        void InFuture(Action<T> action);
    }

    public interface ISynchedFuture
    {
        void Join(Action continuation);
    }
}