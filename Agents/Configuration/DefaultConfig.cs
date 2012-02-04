using System;
using System.Collections.Generic;
using Daemons.MessageBus;
using Daemons.Net;
using Daemons.Reactors;
using Daemons.ServiceLocators;

namespace Daemons.Configuration
{
    public class DefaultConfig : IConfig
    {
        private readonly Dictionary<Type, Action<ServiceLocator>> _registrators = new Dictionary<Type, Action<ServiceLocator>>();
        ServiceLocator _serviceLocator;
        private bool _freezed = false;

        public DefaultConfig()
        {
            RegisterDefaultServices();
        }

        protected virtual void RegisterDefaultServices()
        {
            RegisterService<IDaemonManager, DaemonManager>();
            RegisterService<ITcpService, TcpService>();
            RegisterService<IReactorFactory, ReactorFactory>();
            RegisterService<IReactorInitializer, ReactorInitializer>();
            RegisterService<IDaemonFactory, ThreadPoolDaemonFactory>();
            RegisterService<IMessageBus, SimpleMessageBus>();
        }

        public IConfig RegisterService<TService, TImplementingType>()
        {
            EnsureNotFreezed();
            _registrators[typeof(TService)] = serviceLocator =>
                serviceLocator.RegisterSingleton(typeof(TService), typeof(TImplementingType));
            return this;
        }

        public IConfig RegisterServiceInstance<TService>(TService instance)
        {
            EnsureNotFreezed();
            _registrators[typeof(TService)] = serviceLocator =>
                serviceLocator.RegisterInstance(typeof(TService), instance);
            return this;
        }

        public IDaemonManager BuildManager()
        {
            Freeze();
            return (IDaemonManager) _serviceLocator.GetService(typeof(IDaemonManager));
        }

        public ITcpService BuildTcpService()
        {
            Freeze();
            return (ITcpService)_serviceLocator.GetService(typeof(ITcpService));
        }

        public IServiceLocator BuildServiceLocator()
        {
            Freeze();
            return (IServiceLocator)_serviceLocator.GetService(typeof(IServiceLocator));
        }

        public IMessageBus BuildMessageBus()
        {
            Freeze();
            return (IMessageBus)_serviceLocator.GetService(typeof(IMessageBus));
        }

        private void Freeze()
        {
            if (_freezed == false)
            {
                _freezed = true;
                _serviceLocator = new ServiceLocator();
                _serviceLocator.RegisterInstance(typeof (IServiceProvider), _serviceLocator);
                _serviceLocator.RegisterInstance(typeof(IServiceLocator), _serviceLocator);
                foreach (var registrator in _registrators.Values)
                {
                    registrator(_serviceLocator);
                }
            }
        }
        private void EnsureNotFreezed()
        {
            if(_freezed) throw new InvalidOperationException("Object is freezed. Operation not permited. Cannot register after any service was build.");
        }
    }
}