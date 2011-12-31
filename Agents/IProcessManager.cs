using Agents.Controllers;

namespace Agents
{
    public interface IProcessManager
    {
        IProcess BuildProcess();
        TProcessController BuildProcess<TProcessController>() where TProcessController : ProcessControllerBase;
    }
}