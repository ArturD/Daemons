using System;

namespace Daemons.Reactors
{
    public interface IReactorFactory
    {
        IReactor Build(Type controllerType);
    }
}
