using System.Collections.Generic;
using System.Linq;

namespace Daemons.MQ.Emcaster.Reliability
{
    public class InputMovingWindow
    {
        private readonly Dictionary<int, MessageWithPath> _waitingMessages = new Dictionary<int, MessageWithPath>();
        private int _lastDeliveredMessageNo = 0;
        private int _lastSentMessage = 0;

        public InputMovingWindow()
        {
            _lastDeliveredMessageNo = -1;
        }

        public void AddMessage(string path, MessageBase message)
        {
            if (!OutOfWindow(message)) return;
            lock (this)
            {
                if (!OutOfWindow(message)) return;
                if(!_waitingMessages.ContainsKey(message.MessageNo))
                    _waitingMessages.Add(message.MessageNo,new MessageWithPath(path, message));
                if (_lastDeliveredMessageNo == -1) _lastDeliveredMessageNo = message.MessageNo - 1;
                UpdateLastSentMessage(message.MessageNo);
            }
        }

        private bool OutOfWindow(MessageBase message)
        {
            if (_lastDeliveredMessageNo == -1) return true;
            if (_lastDeliveredMessageNo < message.MessageNo) return true;
            return false;
        }

        public IList<MessageWithPath> Flush()
        {
            var returnedList = new List<MessageWithPath>();
            lock (this)
            {
                MessageWithPath message;
                while (_waitingMessages.TryGetValue(_lastDeliveredMessageNo + 1, out message))
                {
                    returnedList.Add(message);
                    _waitingMessages.Remove(++_lastDeliveredMessageNo);
                }
            }
            return returnedList;
        }

        public IEnumerable<int> GetMissingMessages()
        {
            int lastDelivered;
            int lastSent;
            List<int> presentKeys;
            lock (this)
            {
                lastDelivered = _lastDeliveredMessageNo;
                lastSent = _lastSentMessage;
                if (lastSent == lastDelivered) yield break;
                presentKeys = _waitingMessages.Keys.ToList();
            }
            if(presentKeys.Count == 0 && !(lastSent >0 && lastDelivered > 0))  yield break;
            presentKeys.Sort();
            int pi = 0;
            if (lastSent == -1) lastSent = presentKeys[presentKeys.Count - 1];
            for (int i = lastDelivered + 1; i <= lastSent; i++)
            {
                while (pi < presentKeys.Count && presentKeys[pi] < i) pi++;
                if(pi >= presentKeys.Count || i != presentKeys[pi]) yield return i;
            }
        }

        public void UpdateLastSentMessage(int lastSentMessage)
        {
            lock (this)
            {
                if (_lastSentMessage < lastSentMessage) _lastSentMessage = lastSentMessage;
            }
        }
    }
}