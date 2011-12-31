namespace Agents.Net
{
    public static class TcpServers
    {
        public static TcpServer BuildTcpServer(this IProcessManager processManager, IProcess process)
        {
            var server = new TcpServer(process, processManager);
            return server;
        }
    }
}