using System;

namespace Daemons.ServiceLocators
{
    public interface IServiceRegister
    {
        void RegisterInstance(Type type, object instance);
        void RegisterSingleton(Type resolvingType, Type implementingType);
        void RegisterTransient(Type resolvingType, Type implementingType);
    }
}