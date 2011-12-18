namespace Agents.Net
{
    public static class TcpServers
    {
        public static TcpServer BuildTcpServer(this IScheduler scheduler, IProcess process)
        {
            TcpServer server = new TcpServer(process, new PocessFactory(scheduler));
            return server;
        }
    }
}