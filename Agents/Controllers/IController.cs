namespace Agents.Controllers
{
    public interface IController
    {
        IProcess Process { get; set; }
        void Initialize();
    }
}