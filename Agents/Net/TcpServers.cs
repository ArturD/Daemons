namespace Agents.Net
{
    public static class TcpServers
    {
        public static TcpServer BuildTcpServer(this IProcessFactory processFactory, IProcess process)
        {
            var server = new TcpServer(process, processFactory);
            return server;
        }
    }
}