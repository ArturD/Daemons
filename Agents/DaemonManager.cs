using Agents.Controllers;

namespace Agents
{
    public class DaemonManager : IDaemonManager
    {
        private readonly IControllerFactory _controllerFactory;
        private readonly IControllerInitializer _initializer;
        private readonly IDaemonFactory _daemonFactory;

        public DaemonManager(IControllerFactory controllerFactory, IControllerInitializer initializer, IDaemonFactory daemonFactory)
        {
            _controllerFactory = controllerFactory;
            _initializer = initializer;
            _daemonFactory = daemonFactory;
        }

        public T Build<T>() where T : IController
        {
            var daemon = _daemonFactory.BuildDaemon();
            var controller = (T) _controllerFactory.Build(typeof (T));
            _initializer.Initialize(controller, daemon);
            return controller;
        }
    }
}