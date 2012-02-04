using Daemons.Configuration;

namespace Daemons.MQ
{
    public interface ICompositeMqFluentConfig
    {
        ICompositeMqFluentConfig Add(IMqConfig config);
    }
}