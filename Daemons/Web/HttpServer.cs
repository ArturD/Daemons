using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Daemons.Net;
using Common.Logging;

namespace Daemons.Web
{
    public class HttpServer
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        private readonly IDaemonManager _daemonFactory;
        internal const string HttpNewLine = "\r\n";
        private static readonly Regex Endline = new Regex(HttpNewLine);
        private readonly LowLevelTcpServer _lowLevelTcpServer;
        private Action<IDaemon, HttpRequest, HttpResponse> _httpProcessInitializer;

        public HttpServer(IDaemonManager daemonFactory)
        {
            _daemonFactory = daemonFactory;
            _lowLevelTcpServer = new LowLevelTcpServer(daemonFactory);
        }

        public void Listen(IPEndPoint endpoint, Action<IDaemon, HttpRequest, HttpResponse> httpProcessInitializer)
        {
            _lowLevelTcpServer.Listen(endpoint, TcpProcess);
            _httpProcessInitializer = httpProcessInitializer;
        }

        private void TcpProcess(IDaemon daemon, LowLevelTcpConnection connection)
        {
            Buffer buffer =
                new Buffer()
                    {
                        Segment = new ArraySegment<byte>(new byte[2048])
                    };
            ReadOne(buffer, connection);
        }

        private void ReadOne(Buffer buffer, LowLevelTcpConnection connection)
        {
            connection.ReadAsync(buffer.Segment.Array, buffer.Segment.Offset, buffer.Segment.Count, 
                readBytes =>
                    {
                        var readSegment = new ArraySegment<byte>(buffer.Segment.Array, 0, buffer.Segment.Offset + readBytes);
                        int headersSize;
                        var headers = TryParse(readSegment, out headersSize);
                        if (headers != null)
                        {
                            FixBuffer(buffer.Segment.Array, headersSize, buffer.Segment.Offset + readBytes);
                            StartHttp(headers, buffer.Segment.Array, connection);
                        }
                        else
                        {
                            ReadOne(new Buffer()
                                        {
                                            Segment =
                                                new ArraySegment<byte>(buffer.Segment.Array,
                                                                       buffer.Segment.Offset + readBytes,
                                                                       buffer.Segment.Count - readBytes),
                                        }, connection);
                        }
                    });
        }

        private void FixBuffer(byte[] buffer, int headerBytes, int readBytes)
        {
            for (int i = headerBytes; i < readBytes; i++)
            {
                buffer[i - headerBytes] = buffer[i];
            }
        }

        private string[] TryParse(ArraySegment<byte> segment, out int headersSize)
        {
            headersSize = 0;
            var headersString = Encoding.ASCII.GetString(segment.Array, segment.Offset, segment.Count);
            int endh;
            if ((endh = headersString.IndexOf("\r\n\r\n")) > 0)
            {
                headersSize = endh + 4;
            }
            else if ((endh = headersString.IndexOf("\n\n")) > 0)
            {
                headersSize = endh + 2;
            }
            else
            {
                return null;
            }

            headersString = headersString.Substring(0, headersSize);
            var headers = headersString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (headers.Skip(headers.Length - 2).Count(x => x == "") == 2)
            {
                return headers;
            }
            return null;
        }

        private void StartHttp(string[] headers, byte[] buffer, LowLevelTcpConnection connection)
        {
            _daemonFactory.Spawn(
                (daemon) =>
                    {
                        Logger.Debug("Staring http request processing :"+ headers[0]);
                        var startTime = DateTime.UtcNow;
                        daemon.OnShutdown(() =>
                                               {
                                                   //connection.Process.Shutdown();
                                                   connection.Stream.Flush();
                                                   KeepAlive(buffer, connection);
                                                   Logger.DebugFormat("Ending http request processing {0}", (DateTime.UtcNow - startTime).TotalMilliseconds);
                                               });
                        // todo add killer
                        _httpProcessInitializer(daemon, HttpRequest.Create(headers), new HttpResponse(connection));
                    });
        }

        private void KeepAlive(byte[] buffer, LowLevelTcpConnection connection)
        {
            ReadOne(new Buffer() { Segment = new ArraySegment<byte>(buffer, 0, buffer.Length) }, connection);
        }

        private class Buffer
        {
            public ArraySegment<byte> Segment { get; set; }
        }
    }
}
