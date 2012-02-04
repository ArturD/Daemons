namespace Daemons.Reactors
{
    public interface IReactor
    {
        IDaemon Daemon { get; set; }
        void Initialize();
    }
}