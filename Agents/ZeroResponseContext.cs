using System;

namespace Agents
{
    public class ZeroResponseContext : IMessageContext
    {
        public string Path { get; set; }

        public void Response(object message)
        {
            throw new NotSupportedException();
        }
    }
}