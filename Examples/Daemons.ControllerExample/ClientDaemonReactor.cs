using Daemons;
using Daemons.MessageBus;
using Daemons.Reactors;

namespace Daemons.ControllerExample
{
    public class ClientDaemonReactor : DaemonReactorBase
    {
        private readonly IMessageBus _messageBus;
        private readonly Barrier _barrier;

        public ClientDaemonReactor(IMessageBus messageBus, Barrier barrier)
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
}