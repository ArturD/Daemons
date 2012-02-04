using Emcaster.Topics;

namespace Daemons.MQ.Integration.Emcaster
{
    public interface IEmPublisherFactory
    {
        TopicPublisher CreatePublisher();
    }
}