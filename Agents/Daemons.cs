using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agents.Util;

namespace Agents
{
    public static class Daemons
    {
        private static readonly ThreadLocal<Stack<IProcess>> CurrentStack 
            = new ThreadLocal<Stack<IProcess>>(() => new Stack<IProcess>());

        public static IProcess CurrentOrNull
        {
            get
            {
                var stack = CurrentStack.Value;
                if(stack.Count == 0) return null;
                return stack.Peek();
            }
        }

        /// <summary>
        /// <exception cref="NotInDaemonContextException"></exception>
        /// </summary>
        public static IProcess Current
        {
            get
            {
                var stack = CurrentStack.Value;
                if (stack.Count == 0) throw new NotInDaemonContextException();
                return stack.Peek();
            }
        }

        public static IDisposable Use(IProcess process)
        {
            if (process == null) throw new ArgumentNullException("process");
            CurrentStack.Value.Push(process);
            return new AnonymousDisposer(() => CurrentStack.Value.Pop());
        }
    }
}
