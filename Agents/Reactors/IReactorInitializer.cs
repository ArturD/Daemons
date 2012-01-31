namespace Daemons.Reactors
{
    public interface IReactorInitializer
    {
        void Initialize(IReactor reactor, IDaemon daemon);
    }
}
