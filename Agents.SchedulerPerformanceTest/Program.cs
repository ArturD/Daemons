using System;
using System.Linq;
using System.Threading;
using Daemons;

namespace Agents.SchedulerPerformanceTest
{
    class Program
    {
        private static int _processNo = 1000;
        private static int _messagesPerProcess = 1000;
        private static int _countDown;
        private static int[] _countArray;
        private static DateTime _start;

        static void Main(string[] args)
        {
            if (args.Length >= 1) _processNo = int.Parse(args[0]);
            if (args.Length >= 2) _messagesPerProcess = int.Parse(args[1]);

            _countArray = new int[_processNo];

            _start = DateTime.UtcNow;

            _countDown = _processNo;
            for (int i = 0; i < _processNo; i++)
            {
                int i1 = i; // copy is required, so that closure works as intendent
                var daemon = new ThreadPoolDaemon();
                daemon.Schedule(() => Increment(daemon, i1));
            }
        
            Console.ReadLine();
        }

        private static void Increment(IDaemon daemon, int i)
        {
            _countArray[i]++;
            if (_countArray[i] != _messagesPerProcess)
            {
                daemon.Schedule(() => Increment(daemon, i));
                return;
            }
            if(Interlocked.Decrement(ref _countDown) == 0)
                Console.WriteLine("Time: {0}", (DateTime.UtcNow - _start));
        }
    }
}
