using System;
using Agents.Controllers;
using Agents.MessageBus;

namespace Agents.ControllerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var barrier = new Barrier(2);
            var manager = DaemonConfig.Default()
                .RegisterServiceInstance(barrier)
                .Build();
            manager.Build<ClientDaemonController>();
            manager.Build<PrinterDaemonController>();
            Console.ReadLine();
        }

    }
    public class ClientDaemonController : DaemonControllerBase
    {
        private readonly IMessageBus _messageBus;
        private readonly Barrier _barrier;

        public ClientDaemonController(IMessageBus messageBus, Barrier barrier)
        {
            _messageBus = messageBus;
            _barrier = barrier;
        }

        public override void Initialize()
        {
            _barrier.Join(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var line = "line " + (i + 1);
                    _messageBus.Publish("/printer", line);
                }
            });
        }
    }
    public class PrinterDaemonController : DaemonControllerBase
    {
        private readonly IMessageBus _messageBus;
        private readonly Barrier _barrier;

        public PrinterDaemonController(IMessageBus messageBus, Barrier barrier)
        {
            _messageBus = messageBus;
            _barrier = barrier;
        }

        public override void Initialize()
        {
            _messageBus.Subscribe<string>("/printer", request => Console.WriteLine("Print: " + request));

            _barrier.Join(() => { });
        }
    }
}
