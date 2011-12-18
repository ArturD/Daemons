using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;
using NUnit;
using NUnit.Framework;

namespace Agents.Tests.Util
{
    [TestFixture]
    public class NoWaitQueueTests
    {
        private static IEnumerable<int> Range(int n)
        {
            for (int i = 0; i < n; i++) yield return i;
        }
            
        [Test]
        [Repeat(10)]
        public void SequentialAddSequentialTake()
        {
            int size = 100;

            var queue = new NoWaitQueue<int>();
            foreach(var i in Range(size))
            {
                queue.Add(i);
            }
            for (int i = 0; i < size; i++)
            {
                var result = queue.TakeNoWait();
                Assert.IsTrue(result.Success);
                Assert.AreEqual(i, result.Value);
            }
        }

        [Test]
        [Repeat(10)]
        public void ParallelAddSequentialTake()
        {
            int size = 1000;

            var queue = new NoWaitQueue<int>();
            foreach (var i in Range(size).AsParallel())
            {
                queue.Add(i);
            }
            for (int i = 0; i < size; i++)
            {
                var result = queue.TakeNoWait();
                Assert.IsTrue(result.Success);
                Assert.AreEqual(i, result.Value);
            }
        }

        [Test]
        [Repeat(10)]
        [Timeout(20000)]
        public void ParallelAddTake()
        {
            int size = 100000;
            int[] correct = new int[size];

            var queue = new NoWaitQueue<int>();
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
                while (true)
                {
                    var result = queue.TakeNoWait();
                    if (result.Success)
                    {
                        correct[result.Value]++;
                        break;
                    }
                }
            }
            foreach (var i in Range(size))
            {
                Assert.AreEqual(1, correct[i], "Invalid value at" + i + " = " + correct[i]);
            }
        }
    }
}
