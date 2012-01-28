namespace Agents.Configuration
{
    public interface IConfig
    {
        IConfig RegisterServiceInstance<T>(T barrier);
        IConfig RegisterService<TService, TImplementingType>();
        IDaemonManager Build();
    }
}