using System;

namespace Agents
{


    public class ZeroResponseContext : IMessageContext
    {
        public ZeroResponseContext()
        {
            //Id = id;
            //ResponseToId = responseToId;
        }

        //public string Id { get; private set; }

        //public string ResponseToId { get; private set; }

        public void Response(object message)
        {
            throw new NotSupportedException("Response not supported for this message.");
        }
    }
}