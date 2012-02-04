using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                                                              emcasterConfig.AddRoute("chat");
                                                              emcasterConfig.AddRoute(@"chat/[\w]*");
                                                          })));
            var bus = config.BuildMessageBus();
            var manager = config.BuildManager();

            Console.Write("Enter user name : ");
            var userName = Console.ReadLine();
            Console.Write("Enter channel : #");
            var channel = Console.ReadLine();


            manager.Spawn(daemon =>
                              {
                                  bus.Subscribe<UserJoined>("chat", m => Console.WriteLine("# " + m.UserName + " joined #" + m.Channel));
                                  bus.Subscribe(string.Format(@"chat\{0}", channel), (UserMessage m) => Console.WriteLine("{0}>{1}", m.UserName, m.Message));
                                  bus.Publish("chat", new UserJoined { Channel = channel, UserName = userName});
                              });
            while (true)
            {
                var line = Console.ReadLine();
                bus.Publish(string.Format(@"chat\{0}", channel), new UserMessage(){Message = line, UserName = userName});
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
