namespace Daemons.MQ.Msmq
{
    public interface IMsmqRouteFactory
    {
        IMqRoute CreateRoute(string pattern);
    }
}