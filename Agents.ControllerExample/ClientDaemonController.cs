using Agents.Controllers;
using Agents.MessageBus;

namespace Agents.ControllerExample
{
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
}