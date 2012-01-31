using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daemons;

namespace Agents.MessageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var daemon = new ThreadPoolDaemon();
            var topic = new Topic<string>();

            topic.Subscribe(daemon, (line) => Console.WriteLine(line));
            topic.Publish("Hello World !");
            topic.Publish("Press Any Key ...");
            Console.ReadKey();
        }
    }
}
