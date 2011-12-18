using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents.Web
{
    public static class HttpServers
    {
        public static HttpServer BuildHttpServer(this IScheduler scheduler, IProcess process)
        {
            var server = new HttpServer(process, new PocessFactory(scheduler));
            return server;
        }
    }
}
