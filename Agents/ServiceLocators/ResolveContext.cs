using System;

namespace Daemons.ServiceLocators
{
    public class ResolveContext
    {
        private readonly ResolveContext _outerContext;
        private readonly Type _resolvingType;

        public ResolveContext(ResolveContext outerContext, Type resolvingType)
        {
            _outerContext = outerContext;
            _resolvingType = resolvingType;
        }

        public ResolveContext OuterContext
        {
            get { return _outerContext; }
        }

        public Type ResolvingType
        {
            get { return _resolvingType; }
        }
    }
}