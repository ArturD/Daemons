using System;

namespace Daemons.MQ.Emcaster.Reliability
{
    [Serializable]
    public class MessageBase
    {
        public string ConnectionId { get; set; }
        public int MessageNo { get; set; }
    }
}