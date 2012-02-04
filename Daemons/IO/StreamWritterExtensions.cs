using System;
using System.IO;
using System.Text;

namespace Daemons.IO
{
    public static class StreamWritterExtensions
    {
        public static void Write(this StreamWriter writer, string line, Action continuation)
        {
            var encoding = Encoding.UTF8;
            writer.BaseStream.Write(encoding.GetBytes(line), continuation);
        }
    }
}
