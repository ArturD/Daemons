using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agents.SimpleExample
{
    class Program
    {
        private static IEnumerable<int> Range(int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return i;
            }
        } 

        static void Main(string[] args)
        {
            RunTests();
        }

        private static void RunTests()
        {
            int n = 10000;
            int processNo = 1000;
            using (var scheduler = Schedulers.BuildScheduler())
            {
                var processFactor = new ProcessFactory(scheduler);
                int countDown = processNo;
                var processes = Range(processNo).Select(asd => processFactor.BuildProcess(
                    process =>
                        {
                            int threadsInCount = 0;
                            var ints = new List<int>();
                            process.OnMessage<int>((message, context) =>
                                                       {
                                                           if (Interlocked.Increment(ref threadsInCount) > 1)
                                                               Debug.Fail("More than one thread in single process");
                                                           Debug.Assert(ints.Count == message);
                                                           ints.Add(message);
                                                           
                                                           Debug.Assert(ints.Count <= n);
                                                           if (message == n - 1)
                                                           {
                                                               for (int i = 0; i < ints.Count; i++)
                                                               {
                                                                   if (ints[i] != i)
                                                                       Console.WriteLine("on position {0} was {1}", i, ints[i]);
                                                               }

                                                               Console.WriteLine("one down");
                                                               Interlocked.Decrement(ref countDown);
                                                           }
                                                           else
                                                               process.MessageEndpoint.QueueMessage(message + 1, null);
                                                           Interlocked.Decrement(ref threadsInCount);
                                                       });
                        })).ToArray();

                    foreach (var process in processes)
                    {
                        process.MessageEndpoint.QueueMessage(0, null);
                    }
                while (countDown != 0)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
