using System;

namespace Agents
{
    public static class ProcessManagerExtensions
    {
        public static IProcess BuildProcess(this IProcessManager processManager, Action<IProcess> buildAction)
        {
            var process = processManager.BuildProcess();
            process.Dispatcher.Schedule(() => buildAction(process));
            return process;
        }
    }
}