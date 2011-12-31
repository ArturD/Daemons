using System;
using Agents.Controllers;
using Agents.MessageBus;

namespace Agents
{
    public class ProcessManager : IProcessManager, IDisposable 
    {
        private readonly IScheduler _scheduler;
        private readonly IMessageBusFactory _messageBusFactory;
        private readonly IControllerFactory _controllerFactory;
        private readonly bool _isSchedulerOwner;

        public ProcessManager()
            : this(new DefaultScheduler())
        {
            _isSchedulerOwner = true;
        }

        public ProcessManager(IScheduler scheduler) : this(scheduler, new UnicastMessageBusFactory())
        {
        }

        public ProcessManager(IScheduler scheduler, IMessageBusFactory messageBusFactory) : this(scheduler, messageBusFactory, new ControllerFactory())
        {
        }

        public ProcessManager(IScheduler scheduler, IMessageBusFactory messageBusFactory = null, IControllerFactory controllerFactory = null)
        {
            _scheduler = scheduler;
            _messageBusFactory = messageBusFactory ?? new UnicastMessageBusFactory();
            _controllerFactory = controllerFactory ?? new ControllerFactory();
        }

        public IProcess BuildProcess()
        {
            var process = new Process(new DefaultSchedulerDispatcher(_scheduler), _messageBusFactory);
            return process;
        }

        public TProcessController BuildProcess<TProcessController>() where TProcessController : ProcessControllerBase
        {
            return (TProcessController) _controllerFactory.Build(typeof (TProcessController), BuildProcess());
        }

        public void Dispose()
        {
            if(_isSchedulerOwner)
                _scheduler.Dispose();
        }
    }
}