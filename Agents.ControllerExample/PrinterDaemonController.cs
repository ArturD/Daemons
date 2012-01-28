using System;
using Agents.Controllers;
using Agents.MessageBus;

namespace Agents.ControllerExample
{
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