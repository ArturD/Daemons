Daemons
=======

Simple tcp server example:

```CSharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Daemons.IO;
using Daemons.Net;

namespace Daemons.EchoServerExample
{
    class Program
    {

        static void Main(string[] args)
        {
            var tcpServer = new TcpServer();
            tcpServer.Listen<EchoReactor>(new IPEndPoint(IPAddress.Any, 1234));
            Console.ReadLine();
        }
    }

    class EchoReactor : BufferedReactor
    {
        protected override void NewDataInBuffer()
        {
            var buffer = Buffer.TakeAsArray(Buffer.Count);
            Stream.Write(buffer, () => { });
        }

        protected override void BufferIsFull()
        {
            throw new InvalidOperationException("To much data, must die.");
        }
    }
}
```
