using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Agents.Web
{
    public class HttpRequest
    {
        private static readonly Regex FirstLine = new Regex(@"([a-zA-Z]*) ([a-zA-Z:/0-9~%.,?]*) .*", RegexOptions.Compiled);
        private static readonly Regex Header = new Regex(@"([a-zA-Z]*):[ ]*(.*)", RegexOptions.Compiled);

        public static HttpRequest Create(IEnumerable<string> lines)
        {
            var first = lines.First();
            var firstGroups = FirstLine.Match(first).Groups;
            var firstMatch = firstGroups[2].Value;

            var headers = lines.Skip(1).Where(x=>x.Length > 0).Select(
                x =>
                    {
                        var keyval = Header.Match(x).Groups;
                        if (keyval.Count != 3) throw new FormatException("Bad key/value pair.");
                        return new KeyValuePair<string, string>(keyval[1].Value, keyval[2].Value);
                    }).ToDictionary(x => x.Key, x => x.Value);

            return new HttpRequest()
                       {
                           Path = firstMatch,
                           Headers = headers,
                           MethodVerb = firstGroups[1].Value.ToUpperInvariant()
                       };
        }

        public string MethodVerb { get; set; }
        public string Path { get; set; }
        public IDictionary<string, string> Headers { get; set; } 
    }
}
