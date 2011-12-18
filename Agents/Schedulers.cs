using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public static class Schedulers
    {
        public static DefaultScheduler Default(int threads = 2)
        {
            return new DefaultScheduler(threads);
        }

        public static DefaultSchedulerDispatcher BuildDispatcher(this DefaultScheduler scheduler)
        {
            return new DefaultSchedulerDispatcher(scheduler);
        }

        public static Process BuildProcess(this DefaultScheduler scheduler, Action<Process> initAction)
        {
            var process = new Process(scheduler.BuildDispatcher());
            process.Scheduler.Schedule(() => initAction(process));
            return process;
        }
    }
}
