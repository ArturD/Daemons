namespace Agents
{
    public class ThreadPoolDaemonFactory : IDaemonFactory
    {
        public IDaemon BuildDaemon()
        {
            return new ThreadPoolDaemon();
        }
    }
}