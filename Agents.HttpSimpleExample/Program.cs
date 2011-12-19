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
            try
            {
                using (var scheduler = Schedulers.Default())
                {
                    scheduler.BuildProcess(
                        serverProcess =>
                        {
                            var server = scheduler.BuildHttpServer(serverProcess);
                            server.Listen(new IPEndPoint(IPAddress.Any, 1234),
                                          (process, http) =>
                                              {
                                                  process.Scheduler.ScheduleOne(
                                                      () =>
                                                          {
                                                              http.Write("Hello !", process.Shutdown);
                                                          }, TimeSpan.FromSeconds(1));
                                              });
                        });
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Unexpected end of application.", ex);
            }
        }
    }
}
