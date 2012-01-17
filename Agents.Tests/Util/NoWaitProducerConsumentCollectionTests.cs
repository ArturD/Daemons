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
    public class NoWaitProducerConsumentCollectionTests
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

            var queue = new NoWaitProducerConsumerCollection<int>();
            foreach(var i in Range(size))
            {
                queue.Add(i);
            }
            for (int i = 0; i < size; i++)
            {
                int result;
                Assert.IsTrue(queue.TryTake(out result));
                Assert.AreEqual(i, result);
            }
        }

        [Test]
        [Repeat(10)]
        public void ParallelAddSequentialTake()
        {
            int size = 1000;

            var queue = new NoWaitProducerConsumerCollection<int>();
            foreach (var i in Range(size).AsParallel())
            {
                queue.Add(i);
            }
            for (int i = 0; i < size; i++)
            {
                int result;
                Assert.IsTrue(queue.TryTake(out result));
                Assert.AreEqual(i, result);
            }
        }

        [Test]
        [Repeat(10)]
        [Timeout(20000)]
        public void ParallelAddTake()
        {
            int size = 100000;
            int[] correct = new int[size];

            var queue = new NoWaitProducerConsumerCollection<int>();
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
                    int result;
                    if (queue.TryTake(out result))
                    {
                        correct[result]++;
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
