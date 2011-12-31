using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agents.MessageBus;

namespace Agents.Controllers
{
    public abstract class ProcessControllerBase : IController
    {
        private IProcess _process;

        protected ProcessControllerBase()
        {
        }

        public IProcess Process
        {
            get { return _process; }
            set { _process = value; }
        }

        public IScheduler Scheduler
        {
            get { return _process.Scheduler; }
        }

        public IMessageBus MessageBus
        {
            get { return _process.MessageBus; }
        }

        public IDisposable OnMessage<TMessage>(Action<TMessage, IMessageContext> action)
        {
            return _process.OnMessage(action);
        }

        public IResponseContext SendTo(IProcess targetProcess, object message)
        {
            return _process.SendTo(targetProcess, message);
        }

        public void OnShutdown(Action shutdownAction)
        {
            _process.OnShutdown(shutdownAction);
        }

        public void Schedule(Action action)
        {
            Scheduler.Schedule(action);
        }

        public void ScheduleOne(Action action, TimeSpan delay)
        {
            Scheduler.ScheduleOne(action, delay);
        }

        public void ScheduleInterval(Action action, TimeSpan delay)
        {
            Scheduler.ScheduleInterval(action, delay);
        }

        public IResponseContext Publish(string path, object message)
        {
            return MessageBus.Publish(path, message);
        }

        public void Subscribe<TMessage>(string path, Action<TMessage, IMessageContext> consumer)
        {
            MessageBus.Subscribe(path, consumer);
        }

        public abstract void Initialize();
    }
}
