using System;

namespace Daemons.ServiceLocators
{
    public class InstanceResolver : IResolver
    {
        private readonly Type _forType;
        private readonly object _instance;

        public InstanceResolver(Type forType, object instance)
        {
            _forType = forType;
            _instance = instance;
        }

        public virtual bool CanResolve(ResolveContext context)
        {
            return context.ResolvingType == _forType;
        }

        public virtual object Resolve(ResolveContext context)
        {
            return _instance;
        }
    }
}