using System;
using Agents.Controllers;

namespace Agents.ControllerExample
{
    class Program
    {
        static Barrier _barrier = new Barrier(2);
        static void Main(string[] args)
        {
            var processManager = new ProcessManager();
            var worker = processManager.BuildProcess<PrinterProcessController>();
            var controller = processManager.BuildProcess<ClientProcessController>();
        }

        public class ClientProcessController : ProcessControllerBase
        {
            public override void Initialize()
            {
                _barrier.On(Process).Join(Initialized);
            }

            private void Initialized()
            {
                for (int i = 0; i < 100; i++)
                {
                    int messageNo = i + 1;
                    var line = "line " + messageNo;
                    Publish("/printer", new PrintRequest {Content = line, MessageNo = messageNo}).ExpectResponse<PrintAck>((m, c) => Console.WriteLine("printed: {0}", line));
                }
            }
        }
        public class PrinterProcessController : ProcessControllerBase
        {
            public override void Initialize()
            {
                Subscribe<PrintRequest>("/printer", HandleMessage);

                _barrier.On(Process).Join(()=> { });
            }

            private void HandleMessage(PrintRequest request, IMessageContext messageContext)
            {
                Console.WriteLine("Print: " + request.Content);
                messageContext.Response(new PrintAck());
            }
        }

        class PrintRequest
        {
            public int MessageNo { get; set; }
            public string Content { get; set; }
        }
        class PrintAck
        {
        }
    }
}
