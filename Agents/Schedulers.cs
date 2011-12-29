using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public static class Schedulers
    {
        public static DefaultScheduler BuildScheduler(int threads = 2)
        {
            return new DefaultScheduler(threads);
        }

        public static DefaultSchedulerDispatcher BuildDispatcher(this DefaultScheduler scheduler)
        {
            return new DefaultSchedulerDispatcher(scheduler);
        }
    }

    public static class ProcessFactoryExtensions
    {
        public static IProcess BuildProcess(this IProcessFactory processFactory, Action<IProcess> buildAction)
        {
            var process = processFactory.BuildProcess();
            process.Scheduler.Schedule(() => buildAction(process));
            return process;
        }
    }
}
