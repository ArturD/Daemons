using Daemons.Configuration;

namespace Daemons
{
    public static class DaemonConfig
    {
        public static IConfig Default()
        {
            return new DefaultConfig();
        }
    }
}
