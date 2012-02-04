using System;
using System.IO;

namespace Daemons.IO
{
    public abstract class BufferedReactor : IStreamReactor
    {
        private readonly int _maxBufferSize;
        private ArraySegment<byte>? _bufferLeftover;  
        private readonly ArraysBuffer _buffer = new ArraysBuffer();

        public IDaemon Daemon { get; set; }
        public Stream Stream { get; set; }

        public void Bind(Stream stream)
        {
            Stream = stream;
            StreamBound();
        }

        protected IBufferReader Buffer { get { return _buffer; } }

        protected BufferedReactor() : this(2048)
        {
        }

        protected BufferedReactor(int maxBufferSize)
        {
            _maxBufferSize = maxBufferSize;
        }

        public virtual void Initialize()
        {
            TryReadSome();
        }

        protected virtual void StreamBound()
        {
        }
        protected abstract void NewDataInBuffer();
        protected abstract void BufferIsFull();

        private void TryReadSome()
        {
            var freeSpace = _maxBufferSize - _buffer.Count;
            if(freeSpace <= 0) BufferIsFull();

            var segment = _bufferLeftover ?? new ArraySegment<byte>(new byte[2048]);
            _bufferLeftover = null;

            Stream.Read(segment.Array, segment.Offset, segment.Count,
                        (read) =>
                            {
                                ArraySegment<byte> readSegment;
                                if(segment.Count > read)
                                {
                                    readSegment = new ArraySegment<byte>(segment.Array,
                                                                         segment.Offset,
                                                                         read);
                                    _bufferLeftover = new ArraySegment<byte>(segment.Array,
                                                                             segment.Offset + read,
                                                                             segment.Count - read);
                                }
                                else
                                {
                                    readSegment = segment;
                                }
                                _buffer.Add(readSegment);
                                
                                NewDataInBuffer();
                                TryReadSome();
                            });
        }
    }
}