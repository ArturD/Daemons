using Agents.Configuration;

namespace Agents
{
    public static class DaemonConfig
    {
        public static IConfig Default()
        {
            return new DefaultConfig();
        }
    }
}
