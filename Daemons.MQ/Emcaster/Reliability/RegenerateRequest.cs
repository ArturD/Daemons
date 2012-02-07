using System;

namespace Daemons.MQ.Emcaster.Reliability
{
    [Serializable]
    public class RegenerateRequest
    {
        public string NodeId { get; set; }
        public int MissingMessageNo { get; set; }

        public RegenerateRequest(string nodeId, int missingMessageNo)
        {
            NodeId = nodeId;
            MissingMessageNo = missingMessageNo;
        }
    }
}