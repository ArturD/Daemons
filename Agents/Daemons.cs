﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;

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
