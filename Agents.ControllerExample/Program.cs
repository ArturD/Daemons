using System;
using Agents.Controllers;

namespace Agents.ControllerExample
{
    class Program
    {
        static readonly Barrier Barrier = new Barrier(2);
        private static ProcessManager _processManager;
        static void Main(string[] args)
        {
            _processManager = new ProcessManager();
            var worker = _processManager.BuildProcess<PrinterProcessController>();
            var controller = _processManager.BuildProcess<ClientProcessController>();
        }

        public class ClientProcessController : ProcessControllerBase
        {
            public override void Initialize()
            {
                Barrier.On(Process).Join(() =>
                                             {
                                                 int countDown = 100;
                                                 for (int i = 0; i < 100; i++)
                                                 {
                                                     var line = "line " + (i + 1);
                                                     Publish("/printer", line).
                                                         ExpectResponse<object>(
                                                             (m, c) =>
                                                                 {
                                                                     Console.WriteLine("printed: {0}", line);
                                                                     if(--countDown == 0) _processManager.Dispose();
                                                                 });
                                                 }
                                             });
            }
        }
        public class PrinterProcessController : ProcessControllerBase
        {
            public override void Initialize()
            {
                Subscribe<string>("/printer", (request, context) =>
                                                  {
                                                      Console.WriteLine("Print: " + request);
                                                      context.Response(new object());
                                                  });

                Barrier.On(Process).Join(()=> { });
            }
        }
    }
}
