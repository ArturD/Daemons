using System;
using Daemons;
using Daemons.MessageBus;
using Daemons.Reactors;

namespace Daemons.ControllerExample
{
    public class PrinterDaemonReactor : DaemonReactorBase
    {
        private readonly IMessageBus _messageBus;
        private readonly Barrier _barrier;

        public PrinterDaemonReactor(IMessageBus messageBus, Barrier barrier)
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