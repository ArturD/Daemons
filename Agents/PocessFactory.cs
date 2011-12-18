namespace Agents
{
    class PocessFactory : IPocessFactory
    {
        private readonly IScheduler _scheduler;

        public PocessFactory(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public IProcess BuildProcess()
        {
            var process = new Process(new DefaultSchedulerDispatcher(_scheduler));
            return process;
        }
    }
}