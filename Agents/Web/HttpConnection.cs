using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agents.Net;

namespace Agents.Web
{
    public class HttpConnection
    {
        private TcpConnection _connection;

        public HttpConnection(TcpConnection connection)
        {
            _connection = connection;
        }

        public void Write(string response, Action continuation)
        {
            Write(response, Encoding.ASCII, continuation);
        }

        public void Write(string response, Encoding contentEncoder, Action continuation)
        {
            if (response == null) throw new ArgumentNullException("response");
            if (contentEncoder == null) throw new ArgumentNullException("contentEncoder");
            WriteHeaders(new Dictionary<string, string>
                             {
                                 {"ContentType",  "plain/text"}
                             });
            Write(contentEncoder.GetBytes(response), continuation);
        }

        public void Close()
        {
            _connection.Process.Shutdown();
        }

        private void Write(byte[] bytes)
        {
            Write(bytes, () => { });
        }

        private void Write(byte[] bytes, Action continuation)
        {
            _connection.WriteAsync(bytes, 0, bytes.Length, continuation);
        }

        private void WriteHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            var str =
                "HTTP/1.0 200 OK" + HttpServer.HttpNewLine +
                headers
                    .Select(kv => kv.Key + ": " + kv.Value + HttpServer.HttpNewLine)
                    .Aggregate("", (a, b) => a + b)+ HttpServer.HttpNewLine;

            Write(Encoding.ASCII.GetBytes(str));
        }
    }
}