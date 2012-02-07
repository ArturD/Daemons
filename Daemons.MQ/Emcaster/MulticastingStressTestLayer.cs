using System;

namespace Daemons.MQ.Emcaster
{
    public class MulticastingStressTestLayer : IMulticastingChannel
    {
        private readonly IMulticastingChannel _innerLayer;
        private Random _random;
        private double _dropRate = 0.1;

        public MulticastingStressTestLayer(IMulticastingChannel innerLayer) : this(innerLayer, new Random())
        {
        }

        public MulticastingStressTestLayer(IMulticastingChannel innerLayer, Random random)
        {
            if (innerLayer == null) throw new ArgumentNullException("innerLayer");
            if (random == null) throw new ArgumentNullException("random");

            _innerLayer = innerLayer;
            _random = random;
        }

        public double DropRate
        {
            get { return _dropRate; }
            set
            {
                if (value < 0) throw new ArgumentException("DropRate must be greater or equal 0");
                if (value > 1) throw new ArgumentException("DropRate must be less or equal 1");
                _dropRate = value;
            }
        }

        public void Publish(string path, object message)
        {
            _innerLayer.Publish(path, message);
        }

        public IDisposable Subscribe(string topicPattern, Action<string, object> messageConsumer)
        {
            return _innerLayer.Subscribe(
                topicPattern,
                (path, message) =>
                    {
                        if (_random.NextDouble() > _dropRate)
                            messageConsumer(path, message);
                    });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _innerLayer.Dispose();
        }
    }
}