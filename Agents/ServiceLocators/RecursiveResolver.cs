using System;
using System.Linq;
using System.Reflection;

namespace Agents.ServiceLocators
{
    public class RecursiveResolver : IResolver
    {
        private readonly ServiceLocator _locator;
        private readonly Type _implementedType;
        private readonly Type _implementingType;

        public RecursiveResolver(ServiceLocator locator, Type implementedType, Type implementingType)
        {
            _locator = locator;
            _implementedType = implementedType;
            _implementingType = implementingType;
        }

        protected virtual IOrderedEnumerable<ConstructorInfo> GetConstructorsInOrder()
        {
            return _implementingType
                .GetConstructors()
                .OrderByDescending(constructor => constructor.GetParameters().Count());
        }

        protected virtual bool CanResolveParameter(ResolveContext context, ParameterInfo parameterInfo)
        {
            var type = parameterInfo.ParameterType;
            IResolver resolver = _locator.FindResolver(type);
            if (resolver == null) return false;
            var childContext = new ResolveContext(context, type);
            return resolver.CanResolve(childContext);
        }


        protected virtual object ResolveParameter(ResolveContext context, ParameterInfo parameterInfo)
        {
            var type = parameterInfo.ParameterType;
            IResolver resolver = _locator.FindResolver(type);
            if (resolver == null) return false;
            var childContext = new ResolveContext(context, type);
            return resolver.Resolve(childContext);
        }

        protected virtual bool CheckForCycle(ResolveContext context)
        {
            var current = context.OuterContext;
            while (current != null)
            {
                if (current.ResolvingType == context.ResolvingType) return true;
                current = current.OuterContext;
            }
            return false;
        }

        public bool CanResolve(ResolveContext context)
        {
            if (context.ResolvingType != _implementedType) return false;

            if (CheckForCycle(context)) return false;

            var constructors = GetConstructorsInOrder();

            foreach (var constructorInfo in constructors)
            {
                var parametersInfos = constructorInfo.GetParameters();
                if (parametersInfos.All(parameterInfo => CanResolveParameter(context, parameterInfo)))
                    return true;
            }
            return false;
        }

        public object Resolve(ResolveContext context)
        {
            var constructors = GetConstructorsInOrder();

            foreach (var constructorInfo in constructors)
            {
                var parametersInfos = constructorInfo.GetParameters();

                if (!parametersInfos.All(parameterInfo => CanResolveParameter(context, parameterInfo)))
                    continue;

                var parameters =
                    parametersInfos
                        .Select(parameterInfo => ResolveParameter(context, parameterInfo))
                        .ToArray();

                return constructorInfo.Invoke(parameters);
            }
            throw new InvalidOperationException(string.Format("Cannot resolve type {0}.", context.ResolvingType));
        }
    }
}