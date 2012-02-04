using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons
{
    public class FutureEventEvent : IFutureEvent
    {
        // todo remove locks
        private List<Action> _actions = new List<Action>();
        private int _state = 0;

        public bool Done { get { return _state == 1; } }

        public void Trigger()
        {
            lock (_actions)
            {
                if(_state != 0) return;
                _state = 1;
                foreach (var action in _actions)
                {
                    action();
                }
            }
        }

        public void RegisterHandler(Action continuation)
        {
            var scheduler = Daemons.CurrentScheduler;
            Action scheduledContinuation = () => scheduler.Schedule(continuation);
            lock (_actions)
            {
                if (_state == 1) scheduledContinuation();
                else _actions.Add(scheduledContinuation);
            }
        }
    }
}
