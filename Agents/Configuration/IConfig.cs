using Daemons.MessageBus;
using Daemons.Net;
using Daemons.ServiceLocators;

namespace Daemons.Configuration
{
    public interface IConfig
    {
        IConfig RegisterServiceInstance<T>(T barrier);
        IConfig RegisterService<TService, TImplementingType>();
        IDaemonManager BuildManager();
        ITcpService BuildTcpService();
        IServiceLocator BuildServiceLocator();
        IMessageBus BuildMessageBus();
    }
}