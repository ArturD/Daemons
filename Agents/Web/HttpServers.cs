using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents.Web
{
    public static class HttpServers
    {
        public static HttpServer BuildHttpServer(this IProcessManager processManager, IProcess process)
        {
            var server = new HttpServer(process, processManager);
            return server;
        }
    }
}
