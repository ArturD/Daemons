using System;

namespace Agents.Controllers
{
    public class ControllerFactory : IControllerFactory
    {
        protected virtual IController Constuct(Type controllerType)
        {
            return (IController) Activator.CreateInstance(controllerType);
        }

        public virtual IController Build(Type controllerType, IProcess process)
        {
            var controller = Constuct(controllerType);
            controller.Process = process;
            process.Scheduler.Schedule(controller.Initialize);
            return controller;
        }
    }
}