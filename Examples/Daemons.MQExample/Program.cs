using System;
using Daemons.MQ;

namespace Daemons.MQExample
{
    class Program
    {
        static void Main(string[] args)
        {
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
                            message => Console.WriteLine("# " + message.UserName + " joined #" + message.Channel));
                        bus.Subscribe<UserMessage>(string.Format(@"chat\{0}", channel), 
                            message => Console.WriteLine("{0}>{1}", message.UserName, message.Message));
                        bus.Publish("chat", new UserJoined {Channel = channel, UserName = userName});

                    });
            while (true)
            {
                var line = Console.ReadLine();
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
