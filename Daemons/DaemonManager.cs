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

        public T SpawnWithReactor<T>() where T : IReactor
        {
            return SpawnWithReactor<T>((reactor) => { });
        }

        public T SpawnWithReactor<T>(Action<T> preInitAction) where T : IReactor
        {
            var daemon = _daemonFactory.BuildDaemon();
            var reactor = (T)_reactorFactory.Build(typeof(T));
            preInitAction(reactor);
            _initializer.Initialize(reactor, daemon);
            return reactor;
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
    }
}