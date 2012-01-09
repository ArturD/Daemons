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
                processManager.BuildProcess(
                    serverProcess =>
                        {
                            var server = processManager.BuildHttpServer(serverProcess);
                            server.Listen(new IPEndPoint(IPAddress.Any, 1234),
                                          (process, http) =>
                                              {
                                                  http.Write("Hello !", process.Shutdown);
                                              });
                        });
                Console.ReadLine();
            }
        }
    }
}
