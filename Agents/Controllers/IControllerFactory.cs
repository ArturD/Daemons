using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents.Controllers
{
    public interface IControllerFactory
    {
        IController Build(Type controllerType, IDaemon daemon);
    }
}
