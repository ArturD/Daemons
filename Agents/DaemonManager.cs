using System;
using Daemons.Reactors;

namespace Daemons
{
    public class DaemonManager : IDaemonManager
    {
        private readonly IReactorFactory _reactorFactory;
        private readonly IReactorInitializer _initializer;
        private readonly IDaemonFactory _daemonFactory;

        public DaemonManager(IReactorFactory reactorFactory, IReactorInitializer initializer, IDaemonFactory daemonFactory)
        {
            _reactorFactory = reactorFactory;
            _initializer = initializer;
            _daemonFactory = daemonFactory;
        }

        public IDaemon Spawn()
        {
            return _daemonFactory.BuildDaemon();
        }

        public IDaemon Spawn(Action<IDaemon> initAction)
        {
            var daemon = Spawn();
            daemon.Schedule(() => initAction(daemon));
            return daemon;
        }

        public T SpawnWithReactor<T>() where T : IReactor
        {
            var daemon = _daemonFactory.BuildDaemon();
            var controller = (T) _reactorFactory.Build(typeof (T));
            _initializer.Initialize(controller, daemon);
            return controller;
        }
    }
}