using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Daemons.Net;

namespace Daemons.Web
{
    public class HttpResponse
    {
        private readonly TcpConnection _connection;

        public Stream Stream { get; private set; }

        public HttpResponse(TcpConnection connection)
        {
            _connection = connection;
            Stream = new HttpResponseStream(connection);
        }

        public void WriteAndShutdown(string response)
        {
            WriteAndShutdown(response, () => {});
        }

        public void WriteAndShutdown(string response, Action continuation)
        {
            WriteAndShutdown(response, Encoding.ASCII, continuation);
        }

        public void WriteAndShutdown(string response, Encoding contentEncoder, Action continuation)
        {
            if (response == null) throw new ArgumentNullException("response");
            if (contentEncoder == null) throw new ArgumentNullException("contentEncoder");
            
            var byteContent = contentEncoder.GetBytes(response);

            var headers = FormatHeaders(new Dictionary<string, string>
                                            {
                                                {"ContentType", "text/plain"},
                                                {"Connection", "keep-alive"},
                                                {"Content-Length", byteContent.Length.ToString()},
                                            }).ToString();
            var byteHeaders = contentEncoder.GetBytes(headers);
            var buffer = new byte[byteHeaders.Length + byteContent.Length];
            byteHeaders.CopyTo(buffer, 0);
            byteContent.CopyTo(buffer, byteHeaders.Length);
            WriteAndShutdown(buffer, continuation);
        }

        public void Close()
        {
            _connection.Daemon.Shutdown();
        }

        public void WriteAndShutdown(byte[] bytes)
        {
            WriteAndShutdown(bytes, () => _connection.Daemon.Shutdown());
        }

        public void Write(byte[] bytes)
        {
            WriteAndShutdown(bytes, () => { });
        }

        private void WriteAndShutdown(byte[] bytes, Action continuation)
        {
            _connection.WriteAsync(bytes, 0, bytes.Length, () => { continuation(); _connection.Daemon.Shutdown(); });
        }

        public void WriteHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            var str = FormatHeaders(headers).ToString();
            
            Write(Encoding.ASCII.GetBytes(str));
        }

        private static StringBuilder FormatHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            var builder = new StringBuilder();
            builder.Append("HTTP/1.1 200 OK" + HttpServer.HttpNewLine);
            headers
                .Select(kv => kv.Key + ": " + kv.Value + HttpServer.HttpNewLine)
                .Aggregate(builder, (a, b) => a.Append(b));
            return builder;
        }
    }
}