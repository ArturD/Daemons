using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents.Controllers
{
    public interface IControllerInitializer
    {
        void Initialize(IController controller, IDaemon daemon);
    }
}
