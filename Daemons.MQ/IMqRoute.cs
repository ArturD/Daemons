using System;

namespace Daemons.MQ
{
    public interface IMqRoute : IDisposable
    {
        // consider this for optimisation 
        //bool UseFilteringPattern { get; set; }
        //IEnumerable<string> Patterns { get; set; }

        bool CanPublish<T>(string path, T message);
        void Publish<T>(string path, T message);
        bool CanSubscribe<T>(string path);
        IDisposable Subscribe<T>(string path, Action<T> consumeAction);
    }
}