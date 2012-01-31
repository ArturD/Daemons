using System;
using System.Collections.Generic;
using Daemons.MessageBus;
using Daemons.Reactors;
using Daemons.ServiceLocators;

namespace Daemons.Configuration
{
    public class DefaultConfig : IConfig
    {
        private readonly Dictionary<Type, Action<ServiceLocator>> _registrators = new Dictionary<Type, Action<ServiceLocator>>();  

        public DefaultConfig()
        {
            RegisterService<IDaemonManager, DaemonManager>();
            RegisterService<IReactorFactory, ReactorFactory>();
            RegisterService<IReactorInitializer, ReactorInitializer>();
            RegisterService<IDaemonFactory, ThreadPoolDaemonFactory>();
            RegisterService<IMessageBus, SimpleMessageBus>();
        }

        public IConfig RegisterService<TService, TImplementingType>()
        {
            _registrators[typeof(TService)] = serviceLocator =>
                serviceLocator.RegisterSingleton(typeof(TService), typeof(TImplementingType));
            return this;
        }

        public IConfig RegisterServiceInstance<TService>(TService instance)
        {
            _registrators[typeof(TService)] = serviceLocator =>
                serviceLocator.RegisterInstance(typeof(TService), instance);
            return this;
        }

        public IDaemonManager Build()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterInstance(typeof(IServiceProvider), serviceLocator);
            serviceLocator.RegisterInstance(typeof(IServiceLocator), serviceLocator);
            foreach (var registrator in _registrators.Values)
            {
                registrator(serviceLocator);
            }
            return (IDaemonManager) serviceLocator.GetService(typeof(IDaemonManager));
        }
    }
}