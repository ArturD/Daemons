using System;

namespace Daemons.MQ.Emcaster.Reliability
{
    [Serializable]
    public class Message : MessageBase
    {
        public object InnerObject { get; set; }
    }
}