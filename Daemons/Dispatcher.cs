﻿//using System;
//using System.Collections.Concurrent;
//using System.Threading;
//using Daemons.Util;
//using Common.Logging;

//namespace Daemons
//{
//    public class Dispatcher : IDisposable, IScheduler
//    {
//        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
//        private readonly IDaemon _daemon;
//        private readonly IProducerConsumerCollection<Action> _queue = new NoWaitProducerConsumerCollection<Action>();
//        private readonly IScheduler _scheduler;
//        private int _executing = 0;
//        private bool _disposed = false;

//        public Dispatcher(IScheduler scheduler, IDaemon daemon)
//        {
//            _scheduler = scheduler;
//            _daemon = daemon;
//        }

//        public void Schedule(Action action)
//        {
//            //var processAction = new ProcessAction(action);
//            if (_disposed) return;

//            if (_executing == 0 && Daemons.CurrentOrNull == null && Interlocked.Exchange(ref _executing, 1) == 0)
//            {
//                Logger.Trace("Executing action by stilling thread.");
//                // still thread
//                using (Daemons.Use(_daemon))
//                {
//                    action();
//                }
//                _executing = 0;
//                Thread.MemoryBarrier();
//            }
//            else
//            {
//                Logger.Trace("Queueing action.");
//                _queue.TryAdd(action);

//                // If there is no action on global scheduler, we need to add one.
//                _scheduler.Schedule(DoOne);
//            }

//        }

//        public void ScheduleOne(Action action, TimeSpan delay)
//        {
//            _scheduler.ScheduleOne(() => Schedule(action), delay);
//        }

//        public void ScheduleInterval(Action action, TimeSpan period)
//        {
//            _scheduler.ScheduleInterval(() => Schedule(action), period);
//        }

//        private void DoOne()
//        {
//            if (Interlocked.Exchange(ref _executing, 1) == 0)
//            {
//                Action action;
//                if (_queue.TryTake(out action))
//                {
//                    if (_disposed) return;

//                    using (Daemons.Use(_daemon))
//                    {
//                        action();
//                    }
//                }
//                _executing = 0;
//                Thread.MemoryBarrier();
//            }
//            else _scheduler.Schedule(DoOne);
//        }


//        public void Dispose()
//        {
//            _disposed = true;
//        }
//    }
//}
