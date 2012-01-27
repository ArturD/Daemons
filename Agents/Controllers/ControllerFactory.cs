using System;

namespace Agents.Controllers
{
    public class ControllerFactory : IControllerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ControllerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IController Build(Type controllerType)
        {
            if (controllerType == null) throw new ArgumentNullException("controllerType");
            if(controllerType.IsSubclassOf(typeof(IController))) 
                throw new ArgumentException(string.Format("controllerType ({0}) is not subclass of IController", controllerType));

            var controller = _serviceProvider.GetService(controllerType);

            if(!(controller is IController)) 
                throw new InvalidOperationException("Cannot cast object returned by ServiceProvider to IController. Check IServiceProvider implementation.");

            return (IController) controller;
        }
    }
}