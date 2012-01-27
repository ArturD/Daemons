namespace Agents.Controllers
{
    public class ControllerInitializer : IControllerInitializer
    {
        public void Initialize(IController controller, IDaemon daemon)
        {
            controller.Daemon = daemon;
            daemon.Schedule(controller.Initialize);
        }
    }
}