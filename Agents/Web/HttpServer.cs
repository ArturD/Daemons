using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Agents.Net;
using NLog;

namespace Agents.Web
{
    public class HttpServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IPocessFactory _processFactory;
        internal const string HttpNewLine = "\r\n";
        private static readonly Regex Endline = new Regex(HttpNewLine);
        private readonly TcpServer _tcpServer;
        private Action<IProcess, HttpConnection> _httpProcessInitializer;

        public HttpServer(IProcess process, IPocessFactory processFactory)
        {
            _processFactory = processFactory;
            _tcpServer = new TcpServer(process, processFactory);
        }

        public void Listen(IPEndPoint endpoint, Action<IProcess, HttpConnection> httpProcessInitializer)
        {
            _tcpServer.Listen(endpoint, TcpProcess);
            _httpProcessInitializer = httpProcessInitializer;
        }

        private void TcpProcess(IProcess process, TcpConnection connection)
        {
            Buffer buffer =
                new Buffer()
                    {
                        Segment = new ArraySegment<byte>(new byte[2048])
                    };
            ReadOne(buffer, process, connection);
        }

        private void ReadOne(Buffer buffer, IProcess process, TcpConnection connection)
        {
            connection.ReadAsync(buffer.Segment.Array, buffer.Segment.Offset, buffer.Segment.Count, 
                readBytes =>
                    {
                        var readSegment = new ArraySegment<byte>(buffer.Segment.Array, 0, buffer.Segment.Offset + readBytes);
                        var headers = TryParse(readSegment);
                        if (headers != null)
                        {
                            StartHttp(headers, connection);
                        }
                        else
                        {
                            ReadOne(new Buffer()
                                        {
                                            Segment =
                                                new ArraySegment<byte>(buffer.Segment.Array,
                                                                       buffer.Segment.Offset + readBytes,
                                                                       buffer.Segment.Count - readBytes),
                                        }, process, connection);
                        }
                    });
        }

        private string[] TryParse(ArraySegment<byte> segment)
        {
            var headersString = Encoding.ASCII.GetString(segment.Array, segment.Offset, segment.Count);
            var headers = headersString.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            if (headers.Skip(headers.Length - 2).Count(x => x == "") == 2)
            {
                return headers;
            }
            return null;
        }

        private void StartHttp(string[] headers, TcpConnection connection)
        {
            var process = _processFactory.BuildProcess();
            process.Scheduler.Schedule(
                ()=>
                    {
                        Logger.Debug("Staring http request processing :"+ headers[0]);
                        var startTime = DateTime.UtcNow;
                        process.OnShutdown(() =>
                                               {
                                                   connection.Process.Shutdown();
                                                   Logger.Debug("Ending http request processing {0}", (DateTime.UtcNow - startTime).TotalMilliseconds);
                                               });
                        // todo add killer
                        _httpProcessInitializer(process, new HttpConnection(connection));
                    });
        }

        private class Buffer
        {
            public ArraySegment<byte> Segment { get; set; }
        }
    }
}
