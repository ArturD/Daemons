using Daemons.MQ.Emcaster;

namespace Daemons.MQ.Integration.Emcaster
{
    public class PgmEmMqConfig : EmMqConfigBase
    {
        public PgmEmMqConfig(IMulticastingChannel channel) : base(channel)
        {
        }

        public PgmEmMqConfig(string address, int port)
            : this(new PgmMulticastingChannel(address, port))
        {
        }

        public PgmEmMqConfig AddRoute(string pattern)
        {
            AddRoute(new EmRoute(pattern, Channel));
            return this;
        }
    }
}