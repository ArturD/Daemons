using System;
using System.Net;
using System.Net.Sockets;
using Daemons.Configuration;
using Daemons.MQ.Integration.Emcaster;
using Daemons.MessageBus;

namespace Daemons.MQ
{
    public static class MqDaemonConfigExtensions
    {
        public static IConfig WithMq(this IConfig config, Action<ICompositeMqFluentConfig> mqConfiguration)
        {
            var mqConfig = new CompositeMqFluentConfig();
            mqConfiguration(mqConfig);
            config.RegisterServiceInstance<IMessageBus>(mqConfig.Build());
            return config;
        }

        public static ICompositeMqFluentConfig WithUdpEmCaster(this ICompositeMqFluentConfig config, Action<UdpEmMqConfig> configure)
        {
            return WithUdpEmCaster(config, "225.0.0.69", 6669, configure);
        }

        public static ICompositeMqFluentConfig WithUdpEmCaster(this ICompositeMqFluentConfig config, string address, int port, Action<UdpEmMqConfig> configure)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (address == null) throw new ArgumentNullException("address");
            if (configure == null) throw new ArgumentNullException("configure");
          
            var udpEmMqConfig = new UdpEmMqConfig(address, port);
            configure(udpEmMqConfig);
            config.Add(udpEmMqConfig);
            return config;
        }

        public static ICompositeMqFluentConfig WithPgmEmCaster(this ICompositeMqFluentConfig config, Action<PgmEmMqConfig> configure)
        {
            return WithPgmEmCaster(config, "225.0.0.23", 4002, configure);
        }

        public static ICompositeMqFluentConfig WithPgmEmCaster(this ICompositeMqFluentConfig config, string address, int port, Action<PgmEmMqConfig> configure)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (address == null) throw new ArgumentNullException("address");
            if (configure == null) throw new ArgumentNullException("configure");

            var pgmEmMqConfig = new PgmEmMqConfig(address, port);
            configure(pgmEmMqConfig);
            config.Add(pgmEmMqConfig);
            return config;
        }
    }
}
