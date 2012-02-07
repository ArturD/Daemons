using System.Collections.Generic;
using Daemons.MQ.Emcaster;

namespace Daemons.MQ.Integration.Emcaster
{
    public class EmMqConfigBase : MqConfig
    {
        protected IMulticastingChannel Channel;

        public EmMqConfigBase(IMulticastingChannel channel) : base(new List<IMqRoute>())
        {
            Channel = channel;
        }
    }
}