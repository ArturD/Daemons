using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Daemons.ServiceLocators
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly IDictionary<Type, IResolver> _resolvers = new ConcurrentDictionary<Type, IResolver>();
        
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        /// <param name="serviceType">An object that specifies the type of service object to get. </param><filterpriority>2</filterpriority>
        public object GetService(Type serviceType)
        {
            var resolver = FindResolver(serviceType);
            if (resolver == null) resolver = new RecursiveResolver(this, serviceType, serviceType);

            var context = new ResolveContext(null, serviceType);

            if (!resolver.CanResolve(context)) return null;
            return resolver.Resolve(context);
        }

        public void RegisterInstance(Type type, object instance)
        {
            _resolvers.Add(type, new InstanceResolver(type, instance));
        }

        public void RegisterSingleton(Type resolvingType, Type implementingType)
        {
            _resolvers.Add(resolvingType,
                           new SingletonLifetimeResolverWrapper(
                               new RecursiveResolver(this, resolvingType, implementingType)));
        }

        public void RegisterTransient(Type resolvingType, Type implementingType)
        {
            _resolvers.Add(resolvingType,
                           new RecursiveResolver(this, resolvingType, implementingType));
        }

        public virtual IResolver FindResolver(Type type)
        {
            IResolver resolver;
            if (_resolvers.TryGetValue(type, out resolver)) return resolver;
            return null;
        }

    }
}