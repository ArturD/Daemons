using System;
using System.Linq;
using System.Threading;
using Daemons;

namespace Agents.MessagePerformanceTest
{
    class Program
    {
        private static int _processNo = 1000;
        private static int _messagesPerProcess = 1000;
        private static int _countDown;
        private static DateTime _start;

        static void Main(string[] args)
        {
            if (args.Length >= 1) _processNo = int.Parse(args[0]);
            if (args.Length >= 2) _messagesPerProcess = int.Parse(args[1]);

            _start = DateTime.UtcNow;

            _countDown = _processNo;
            for (int i = 0; i < _processNo; i++)
            {
                Topic<int> topic = new Topic<int>();
                var daemon = new ThreadPoolDaemon();
                topic.Subscribe(daemon, (msg) => Increment(topic, msg));
                topic.Publish(0);
            }

            Console.ReadLine();
        }

        private static void Increment(IPublisher<int> publisher, int msg)
        {
            if (msg != _messagesPerProcess)
            {
                publisher.Publish(msg + 1);
                return;
            }
            if (Interlocked.Decrement(ref _countDown) == 0)
                Console.WriteLine("Time: {0}", (DateTime.UtcNow - _start));
        }
    }
}
