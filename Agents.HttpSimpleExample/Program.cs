using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Agents.IO;
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
                            Logger.Info("initialize http server !");
                            server.Listen(new IPEndPoint(IPAddress.Any, 1234),
                                          (process, request, response) =>
                                              {
                                                  if (request.Path == "/index.html")
                                                  {
                                                      response.WriteHeaders(new Dictionary<string, string>() { {"content-type", "text/html"}});
                                                      var stream = File.OpenRead("index.html");
                                                      stream.Pipe(response.Stream, ()=>
                                                                                       {
                                                                                           stream.Dispose();
                                                                                           process.Shutdown();
                                                                                       });
                                                  }
                                                  else
                                                  {
                                                      response.WriteAndShutdown(string.Format("Hello ! \nfrom {0} {1}",
                                                                                              request.MethodVerb,
                                                                                              request.Path));
                                                  }
                                              });
                        });
                Console.ReadLine();
            }
        }
    }
}
