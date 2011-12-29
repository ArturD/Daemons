using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Agents.Net;

namespace Agents.EchoServerExample
{
    class Program
    {

        static void Main(string[] args)
        {
            using (var scheduler = Schedulers.BuildScheduler())
            {
                var processFactory = new ProcessFactory(scheduler);
                processFactory.BuildProcess(
                    serverProcess =>
                        {
                            var server = processFactory.BuildTcpServer(serverProcess);
                            server.Listen(new IPEndPoint(IPAddress.Any, 1234), 
                                (process, tcp) =>
                                    {
                                        byte[] buffer = new byte[1024];
                                        ReadOne(tcp, buffer);
                                    });
                        });
                Console.ReadLine();
            }
        }

        private static void ReadOne(TcpConnection tcp, byte[] buffer)
        {
            tcp.ReadAsync(buffer, 0, buffer.Length,
                     readCount =>
                         {
                             WriteOne(tcp, buffer, readCount);
                             ReadOne(tcp, buffer);
                         });
        }

        private static void WriteOne(TcpConnection tcp, byte[] buffer, int readCount)
        {
            var bufferCopy = (byte[])buffer.Clone();
            tcp.WriteAsync(bufferCopy, 0, readCount,
                () => { });
        }
    }
}
