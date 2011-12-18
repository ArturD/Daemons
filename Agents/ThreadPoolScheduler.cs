using System;
using System.Threading;

namespace Agents
{
    public class ThreadPoolScheduler : IScheduler
    {
        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
        }

        public void Schedule(Action action)
        {
            if(_disposed) return;
            ThreadPool.QueueUserWorkItem(o => action());
        }
    }
}