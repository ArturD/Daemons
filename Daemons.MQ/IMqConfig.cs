using System.Collections.Generic;

namespace Daemons.MQ
{
    public interface IMqConfig
    {
        IEnumerable<IMqRoute> Routes();
    }
}