namespace Daemons.MQ.Msmq
{
    public interface IMsmqServiceFactory
    {
        IMsmqService Build();
    }
}