using System;

namespace Daemons.MQ.Emcaster.Reliability
{
    [Serializable]
    public class HeartBeatMessage
    {
        public HeartBeatMessage(string connectionId, int lastSentMessage)
        {
            ConnectionId = connectionId;
            LastSentMessage = lastSentMessage;
        }

        public string ConnectionId { get; set; }
        public int LastSentMessage { get; set; }
    }
}