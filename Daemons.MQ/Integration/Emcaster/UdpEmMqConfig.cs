using System;
using System.Collections.Generic;
using Daemons.MQ.Emcaster;
using Daemons.MQ.Emcaster.Reliability;

namespace Daemons.MQ.Integration.Emcaster
{
    public class UdpEmMqConfig : EmMqConfigBase
    {
        private bool _anyRoutesAlreadyAdded = false;
        public UdpEmMqConfig(string address, int port) 
            : base(new UdpMulticastingChannel(address, port))
        {
        }

        public UdpEmMqConfig AddStressTestLayer()
        {
            return AddStressTestLayer(0.9);
        }

        public UdpEmMqConfig AddStressTestLayer(double dropRate)
        {
            if (_anyRoutesAlreadyAdded) throw new InvalidOperationException("You must add all layers before before any route");
            Channel = new MulticastingStressTestLayer(Channel)
                          {
                              DropRate = dropRate,
                          };
            return this;
        }

        public UdpEmMqConfig AddReliabilityLayer()
        {
            return AddReliabilityLayer(TimeSpan.FromMilliseconds(1000));
        }

        public UdpEmMqConfig AddReliabilityLayer(TimeSpan heartbeat)
        {
            if (_anyRoutesAlreadyAdded) throw new InvalidOperationException("You must add all layers before before any route");
            Channel = new MulticastingChannelRealiabilityLayer(Channel, heartbeat);
            //Channel = new MulticastingChannelHeartbeatLayer(Channel, heartbeat);
            return this;
        }

        public UdpEmMqConfig AddRoute(string pattern)
        {
            _anyRoutesAlreadyAdded = true;
            AddRoute(new EmRoute(pattern, Channel));
            return this;
        }
    }
}
