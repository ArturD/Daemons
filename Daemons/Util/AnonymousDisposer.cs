using System;

namespace Daemons.Util
{
    public class AnonymousDisposer : IDisposable
    {
        private readonly Action _disposeAction;

        public AnonymousDisposer(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}
