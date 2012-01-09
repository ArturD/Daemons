using System;
using System.Runtime.Serialization;

namespace Agents
{
    [Serializable]
    public class NotInDaemonContextException : Exception
    {
        public NotInDaemonContextException()
            : this("Operation should be called within context of daemon.")
        {
        }

        public NotInDaemonContextException(string message) : base(message)
        {
        }

        public NotInDaemonContextException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NotInDaemonContextException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}