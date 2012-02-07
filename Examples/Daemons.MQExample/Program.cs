using System;
using System.Linq;
using Common.Logging;
using Common.Logging.Simple;
using Daemons.MQ;

namespace Daemons.MQExample
{
    class Program
    {
        static void Main(string[] args)
        {
            //LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(new NameValueCollection());
            var config = DaemonConfig
                .Default()
                .WithMq((conf => conf.WithUdpEmCaster(emcasterConfig =>
                                                          {
                                                              emcasterConfig.AddStressTestLayer(0.1);
                                                              emcasterConfig.AddReliabilityLayer(TimeSpan.FromSeconds(1));
                                                              emcasterConfig.AddRoute("chat");
                                                              emcasterConfig.AddRoute(@"chat/[\w]*");
                                                          })));
            var bus = config.BuildMessageBus();
            
            Console.Write("Enter user name : ");
            var userName = Console.ReadLine();
            Console.Write("Enter channel : #");
            var channel = Console.ReadLine();

            var daemon = new ThreadPoolDaemon();
            daemon.Schedule(
                () =>
                    {
                        bus.Subscribe<UserJoined>("chat",
                                                  m => Console.WriteLine("# " + m.UserName + " joined #" + m.Channel));

                        bus.Subscribe(string.Format(@"chat\{0}", channel),
                                      (UserMessage m) => Console.WriteLine("{0}>{1}", m.UserName, m.Message));

                        bus.Publish("chat", new UserJoined {Channel = channel, UserName = userName});

                    });
            while (true)
            {
                var line = Console.ReadLine();
                int times = 1;
                if (!int.TryParse(line.Split(' ').FirstOrDefault() ?? "", out times)) times = 1;
                for (int i = 0; i < times; i++)
                    bus.Publish(string.Format(@"chat\{0}", channel), new UserMessage() { Message = line, UserName = userName });
            }
        }

        [Serializable]
        public class UserJoined
        {
            public string UserName { get; set; }
            public string Channel { get; set; }
        }

        [Serializable]
        public class UserMessage
        {
            public string UserName { get; set; }
            public string Message { get; set; }
        }
    }
}
