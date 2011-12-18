using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public static class Schedulers
    {
        public static IScheduler Default(int threads = 2)
        {
            return new ThreadPoolScheduler();
        }

        public static DefaultSchedulerDispatcher BuildDispatcher(this IScheduler scheduler)
        {
            return new DefaultSchedulerDispatcher(scheduler);
        }

        public static Process BuildProcess(this IScheduler scheduler, Action<Process> initAction)
        {
            var process = new Process(scheduler.BuildDispatcher());
            process.Scheduler.Schedule(() => initAction(process));
            return process;
        }
    }
}
