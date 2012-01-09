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
            const int n = 1000;
            const int processNo = 1000;
            using (var processManager = new ProcessManager())
            {
                DateTime start = DateTime.UtcNow;
                DateTime end = DateTime.MinValue;
                int  countDown = processNo;
                var processes = Range(processNo).Select(asd => processManager.BuildProcess(
                    process =>
                        {
                            var ints = new List<int>();
                            process.OnMessage<int>("/ints",(message, context) =>
                                                       {
                                                           ints.Add(message);
                                                           
                                                           if (message == n - 1)
                                                           {
                                                               for (int i = 0; i < ints.Count; i++)
                                                               {
                                                                   if (ints[i] != i)
                                                                       Console.WriteLine("on position {0} was {1}", i, ints[i]);
                                                               }

                                                               Console.WriteLine("one down");
                                                               if (Interlocked.Decrement(ref countDown) == 0)
                                                                   end = DateTime.UtcNow;
                                                           }
                                                           else
                                                               process.MessageEndpoint.QueueMessage(message + 1, null);
                                                       });
                        })).ToArray();

                    foreach (var targetProcess in processes)
                    {
                        targetProcess.MessageEndpoint.QueueMessage(0, new ZeroResponseContext());
                    }
                while (countDown != 0)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("Time: {0}", (end - start));
                Console.ReadLine();
            }
        }
    }
}
