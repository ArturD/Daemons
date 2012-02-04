using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Daemons.Util;
using NUnit.Framework;

namespace Daemons.Tests.Util
{
    public class WaitQueueTests
    {

        private static IEnumerable<int> Range(int n)
        {
            for (int i = 0; i < n; i++) yield return i;
        }

        [Test]
        [Repeat(100)]
        [Timeout(20000)]
        public void ConcurrentAddAndTestTest()
        {
            int size = 1000;
            int[] correct = new int[size];

            var queue = new WaitQueue<int>();
            ThreadPool.QueueUserWorkItem(
                x =>
                {
                    foreach (var i in Range(size).AsParallel())
                    {
                        queue.Add(i);
                    }
                });

            foreach (var i in Range(size).AsParallel())
            {
                var result = queue.Take();
                correct[result]++;
            }
            foreach (var i in Range(size))
            {
                Assert.AreEqual(1, correct[i], "Invalid value at" + i + " = " + correct[i]);
            }
        }
    }
}
