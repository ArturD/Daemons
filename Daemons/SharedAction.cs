using System;
using System.Diagnostics;
using System.Threading;

namespace Daemons
{
    public class SharedAction
    {
        private readonly Action _action;
        // 0 - initial, 1 - executing, 2 - executed, -1 - aborted
        private int _state;

        public bool Executing { get { return _state == 1; } }
        public bool Executed { get { return _state == 2; } }
        public bool ExecutingOrExecuted { get { return _state > 0; } }
        public bool Aborted { get { return _state == -1; } }

        public SharedAction(Action action)
        {
            _action = action;
        }

        public bool TryExecute()
        {
            if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
            {
                _action();
                Debug.Assert(_state == 1);
                _state = 2;
                return true;
            }
            return false;
        }
        public bool TryAbort()
        {
            if (Interlocked.CompareExchange(ref _state, -1, 0) == 0)
            {
                return true;
            }
            return false;
        }
    }
}
