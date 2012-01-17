using System;

namespace Agents.Controllers
{
    public class ControllerFactory : IControllerFactory
    {
        protected virtual IController Constuct(Type controllerType)
        {
            return (IController) Activator.CreateInstance(controllerType);
        }

        public virtual IController Build(Type controllerType, IDaemon daemon)
        {
            var controller = Constuct(controllerType);
            controller.Daemon = daemon;
            daemon.Schedule(controller.Initialize);
            return controller;
        }
    }
}