using System;
using System.Collections.Generic;

namespace Agents
{
    public static class Daemons
    {
        [ThreadStatic] private static Stack<IDaemon> _currentStack;

        public static IDaemon CurrentOrNull
        {
            get
            {
                if (_currentStack == null || _currentStack.Count == 0) return null;
                return _currentStack.Peek();
            }
        }

        /// <summary>
        /// <exception cref="NotInDaemonContextException"></exception>
        /// </summary>
        public static IDaemon Current
        {
            get
            {
                if (_currentStack == null || _currentStack.Count == 0) throw new NotInDaemonContextException();
                return _currentStack.Peek();
            }
        }

        public static IScheduler CurrentScheduler
        {
            get
            { 
                var daemon = Current;
                if (daemon == null) return ThreadPoolScheduler.Instance;
                return daemon;
            }
        }

        public static IDisposable Use(IDaemon daemon)
        {
            if (daemon == null) throw new ArgumentNullException("daemon");
            if(_currentStack == null) _currentStack = new Stack<IDaemon>();
            _currentStack.Push(daemon);
            return UseDisposer.Instance;
        }

        private class UseDisposer : IDisposable
        {
            public static UseDisposer Instance = new UseDisposer();

            public void Dispose()
            {
                _currentStack.Pop();
            }
        }
    }
}
