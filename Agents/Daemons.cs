using System;
using System.Collections.Generic;

namespace Agents
{
    public static class Daemons
    {
        [ThreadStatic] private static Stack<IProcess> _currentStack;

        public static IProcess CurrentOrNull
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
        public static IProcess Current
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
                var process = Current;
                if (process == null) return ThreadPoolScheduler.Instance;
                return process.Dispatcher;
            }
        }

        public static IDisposable Use(IProcess process)
        {
            if (process == null) throw new ArgumentNullException("process");
            if(_currentStack == null) _currentStack = new Stack<IProcess>();
            _currentStack.Push(process);
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
