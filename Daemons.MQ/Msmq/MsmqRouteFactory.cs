namespace Daemons.MQ.Msmq
{
    public class MsmqRouteFactory : IMsmqRouteFactory
    {
        private readonly IMsmqServiceFactory _serviceFactory;
        private IMsmqService _service;

        public MsmqRouteFactory(IMsmqServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public IMqRoute CreateRoute(string pattern)
        {
            if (_service == null) _service = _serviceFactory.Build();

            return new StaticMsmqRoute(_service, pattern);
        }
    }
}