namespace Daemons.Reactors
{
    public class ReactorInitializer : IReactorInitializer
    {
        public void Initialize(IReactor reactor, IDaemon daemon)
        {
            reactor.Daemon = daemon;
            daemon.Schedule(reactor.Initialize);
        }
    }
}