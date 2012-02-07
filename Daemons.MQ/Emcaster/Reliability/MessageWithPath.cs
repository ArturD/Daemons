namespace Daemons.MQ.Emcaster.Reliability
{
    public class MessageWithPath
    {
        public MessageWithPath(string path, MessageBase message)
        {
            Message = message;
            Path = path;
        }

        public MessageBase Message { get; set; }
        public string Path { get; set; }
    }
}