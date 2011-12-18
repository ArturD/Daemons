using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public interface IScheduler : IDisposable
    {
        void Schedule(Action action);
    }
}
