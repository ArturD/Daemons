using System;
using System.Collections.Generic;

namespace Daemons.MQ
{
    public class MqConfig : IMqConfig
    {
        private readonly List<IMqRoute> _routes;
        private bool _frozen = false;

        public MqConfig(List<IMqRoute> routes)
        {
            _routes = routes;
        }

        protected virtual void AddRoute(IMqRoute route)
        {
            if (route == null) throw new ArgumentNullException("route");
            if (_frozen) throw new InvalidOperationException("Object is frozen. Cannot configure any more.");
            _routes.Add(route);
        }

        public IEnumerable<IMqRoute> Routes()
        {
            _frozen = true;
            return _routes;
        }
    }
}