using System;
using System.Runtime.InteropServices;

namespace Emcaster.Topics
{
    public struct MessageHeader
    {
        private int _topicSize;
        private int _bodySize;

        public MessageHeader(int topicSize, int bodySize)
        {
            _topicSize = topicSize;
            _bodySize = bodySize;
        }

        public int TopicSize
        {
            get { return _topicSize; }
        }

        public int BodySize
        {
            get { return _bodySize; }
        }

        public int TotalSize
        {
            get { return _topicSize + _bodySize; }
        }

        public void WriteToBuffer(byte[] allData)
        {
            BitConverter.GetBytes(_bodySize).CopyTo(allData, 0);
            BitConverter.GetBytes(_topicSize).CopyTo(allData, 4);
        }
    }
}