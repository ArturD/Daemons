using Agents.Controllers;

namespace Agents
{
    public interface IDaemonManager
    {
        T Build<T>() where T : IController;
    }
}