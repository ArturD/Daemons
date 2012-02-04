using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons.MQ.Msmq
{
    public class MsmqFluentConfigurator
    {
        private readonly List<Func<IMqRoute>> _routeBuilders = new List<Func<IMqRoute>>();
        private readonly IMsmqRouteFactory _routeFactory;

        public MsmqFluentConfigurator(IMsmqRouteFactory routeFactory)
        {
            _routeFactory = routeFactory;
        }

        public virtual MsmqFluentConfigurator AddRoute(string pattern)
        {
            _routeBuilders.Add(() => _routeFactory.CreateRoute(pattern));
            return this;
        }

        public virtual IMqConfig BuildConfig()
        {
            return new MqConfig(_routeBuilders
                                      .Select(build => build())
                                      .ToList());
        }
    }
}
