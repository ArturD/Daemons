using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agents.Controllers;

namespace Agents.ControllerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var processManager = new ProcessManager();
            var worker = processManager.BuildProcess<PrinterProcessController>();
            var controller = processManager.BuildProcess<ProcessController>();
        }

        class ProcessController : ProcessControllerBase
        {
            public override void Initialize()
            {
                for (int i = 0; i < 1000; i++)
                {
                    int messageNo = i;
                    var line = "line " + i;
                    MessageBus.Publish("/printer", new PrintRequest() { Content = line, MessageNo = messageNo })
                        .ExpectMessage<PrintAck>((m, c) =>
                                                     {
                                                         if (messageNo % 100 == 0) Console.WriteLine("printed: {0}", line);
                                                     });
                }
            }
        }
        class PrinterProcessController : ProcessControllerBase
        {
            public override void Initialize()
            {
                Subscribe<PrintRequest>("/printer", HandleMessage);
            }

            private void HandleMessage(PrintRequest request, IMessageContext messageContext)
            {
                if(request.MessageNo % 100 == 0)Console.WriteLine("Print: " + request.Content);
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
