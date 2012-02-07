using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emcaster.Sockets;

namespace Daemons.MQ.Emcaster.Sockets
{
    public class DaemonWritter : IByteWriter 
    {
        private readonly ThreadPoolDaemon _daemon = new ThreadPoolDaemon();
        private readonly UdpSource _source;
        private Queue<Action> _actionQueue = new Queue<Action>(); 

        public DaemonWritter(UdpSource source)
        {
            _source = source;
        }

        public bool Write(byte[] data, int offset, int length, int msToWaitForWriteLock)
        {
            byte[] copy = new byte[length];
            Array.Copy(data, offset, copy, 0, length);
            Schedule(() => BeginWrite(length, copy));
            return true;
        }

        private void Schedule(Action writeAction)
        {
            _daemon.Schedule(() =>
                                 {
                                     _actionQueue.Enqueue(writeAction);
                                     if (_actionQueue.Count == 1) FlushOne();
                                 });
        }

        private void FlushOne()
        {
            if (_actionQueue.Count == 0) return;
            var action = _actionQueue.Dequeue();
            action();
        }

        private IAsyncResult BeginWrite(int length, byte[] copy)
        {
            return _source.BeginWrite(
                copy,
                0,
                length,
                EndWrite,
                null);
        }

        private void EndWrite(IAsyncResult ar)
        {
            int result = _source.EndWrite(ar);
            FlushOne();
        }

        public void Start()
        {
        }
    }
}
