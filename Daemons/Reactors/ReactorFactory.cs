using System;

namespace Daemons.Reactors
{
    public class ReactorFactory : IReactorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ReactorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReactor Build(Type controllerType)
        {
            if (controllerType == null) throw new ArgumentNullException("controllerType");
            if(controllerType.IsSubclassOf(typeof(IReactor))) 
                throw new ArgumentException(string.Format("controllerType ({0}) is not subclass of IController", controllerType));

            var controller = _serviceProvider.GetService(controllerType);

            if(!(controller is IReactor)) 
                throw new InvalidOperationException("Cannot cast object returned by ServiceProvider to IController. Check IServiceProvider implementation.");

            return (IReactor) controller;
        }
    }
}