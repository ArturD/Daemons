namespace Agents.Configuration
{
    public interface IConfig
    {
        IConfig RegisterService<TService, TImplementingType>();
        IDaemonManager Build();
    }
}