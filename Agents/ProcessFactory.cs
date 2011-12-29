using System;
using Agents.MessageBus;

namespace Agents
{
    public class ProcessFactory : IProcessFactory, IDisposable 
    {
        private readonly IScheduler _scheduler;
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly bool _isSchedulerOwner;

        public ProcessFactory()
            : this(new DefaultScheduler())
        {
            _isSchedulerOwner = true;
        }

        public ProcessFactory(IScheduler scheduler) : this(scheduler, new UnicastMessageBusFactory())
        {
        }

        public ProcessFactory(IScheduler scheduler, IMessageBusFactory messageBusFactory)
        {
            _scheduler = scheduler;
            _messageBusFactory = messageBusFactory;
        }

        public IProcess BuildProcess()
        {
            var process = new Process(new DefaultSchedulerDispatcher(_scheduler), _messageBusFactory);
            return process;
        }

        public void Dispose()
        {
            if(_isSchedulerOwner)
                _scheduler.Dispose();
        }
    }
}