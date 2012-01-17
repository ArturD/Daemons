namespace Agents.Controllers
{
    public interface IController
    {
        IDaemon Daemon { get; set; }
        void Initialize();
    }
}