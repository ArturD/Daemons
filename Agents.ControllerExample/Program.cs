using System;
using Agents.Controllers;
using Agents.MessageBus;

namespace Agents.ControllerExample
{
    class Program
    {
        static readonly Barrier Barrier = new Barrier(2);
        static void Main(string[] args)
        {
            var manager = DaemonConfig.Default().Build();
            manager.Build<ClientProcessController>();
            manager.Build<PrinterProcessController>();
            Console.ReadLine();
        }

        public class ClientProcessController : DaemonControllerBase
        {
            private readonly IMessageBus _messageBus;

            public ClientProcessController(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public override void Initialize()
            {
                Barrier.Join(() =>
                                 {
                                     for (int i = 0; i < 100; i++)
                                     {
                                         var line = "line " + (i + 1);
                                         _messageBus.Publish("/printer", line);
                                     }
                                 });
            }
        }
        public class PrinterProcessController : DaemonControllerBase
        {
            private readonly IMessageBus _messageBus;

            public PrinterProcessController(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public override void Initialize()
            {
                _messageBus.Subscribe<string>("/printer", request => Console.WriteLine("Print: " + request));

                Barrier.Join(() => { });
            }
        }
    }
}
