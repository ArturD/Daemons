namespace Daemons.ServiceLocators
{
    public class SingletonLifetimeResolverWrapper : IResolver
    {
        private object _instance;
        private IResolver _innerResolver;

        public SingletonLifetimeResolverWrapper(IResolver innerResolver)
        {
            _innerResolver = innerResolver;
        }

        public bool CanResolve(ResolveContext context)
        {
            if (_instance != null) return true;
            return _innerResolver.CanResolve(context);
        }

        public object Resolve(ResolveContext context)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                    {
                        _instance = _innerResolver.Resolve(context);
                    }
                }
            }
            return _instance;
        }
    }
}
