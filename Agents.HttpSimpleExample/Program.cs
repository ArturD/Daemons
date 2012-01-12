using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Agents.Web;
using NLog;

namespace Agents.HttpSimpleExample
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            using (var processManager = new ProcessManager())
            {
                processManager.BuildHttpServerProcess(
                    (serverProcess, server) =>
                        {
                            server.Listen(new IPEndPoint(IPAddress.Any, 1234),
                                          (process, request, response) =>
                                              {
                                                  response.WriteAndShutdown(string.Format("Hello ! \nfrom {0} {1}", request.MethodVerb, request.Path));
                                              });
                        });
                Console.ReadLine();
            }
        }
    }
}
