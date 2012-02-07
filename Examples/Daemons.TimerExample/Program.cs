using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons.TimerExample
{
    class Program
    {
        private static ThreadPoolDaemon _daemon;
        static void Main(string[] args)
        {
            _daemon = new ThreadPoolDaemon();
            GoOne();
            Console.ReadKey();
        }

        private static void GoOne()
        {
            Console.WriteLine("tick");
            _daemon.ScheduleOne(GoOne, TimeSpan.FromMilliseconds(10));
        }
    }
}
