using System;
using System.Net;
using Daemons.IO;

namespace Daemons.Net
{
    public interface ITcpService
    {
        void Connect<T>(IPEndPoint endpoint) where T : IStreamReactor;
        void Connect<T>(IPEndPoint endpoint, Action<Exception> faild) where T : IStreamReactor;
        void Connect<T>(IPEndPoint endpoint, Action<T> success, Action<Exception> faild) where T : IStreamReactor;
    }
}