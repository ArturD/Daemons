using System;
using System.IO;
using System.Net.Sockets;

namespace Daemons.IO
{
    public class StreamPipe
    {
        private readonly Stream _source;
        private readonly Stream _destination;
        private readonly byte[] _buffer;
        private readonly Action _finish;
        private readonly Action<Exception> _error;

        public StreamPipe(Stream source, Stream destination, byte[] buffer, Action finish, Action<Exception> error)
        {
            _source = source;
            _destination = destination;
            _buffer = buffer;
            _finish = finish;
            _error = error;
        }

        public void Start()
        {
            BeginRead();
        }

        private void BeginRead()
        {
            if (_source is NetworkStream)
            {
                if (((NetworkStream)_source).DataAvailable)
                {
                    // if there is data in buffer, read it synchronously.
                    try
                    {
                        int read = _source.Read(_buffer, 0, _buffer.Length);
                        ConsumeRead(read);
                    }
                    catch (IOException ex)
                    {
                        _error(ex);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        _error(ex);
                    }
                    return;
                }
            }
            try
            {
                _source.BeginRead(_buffer, 0, _buffer.Length, EndRead, null);
            }
            catch (IOException ex)
            {
                _error(ex);
            }
            catch (ObjectDisposedException ex)
            {
                _error(ex);
            }
        }

        private void EndRead(IAsyncResult ar)
        {
            var read = _source.EndRead(ar);
            ConsumeRead(read);
        }

        private void ConsumeRead(int read)
        {
            if (read == 0)
            {
                _finish();
            }
            else
            {
                BeginWrite(read);
            }
        }

        private void BeginWrite(int read)
        {
            try
            {
                _destination.BeginWrite(_buffer, 0, read, EndWrite, null);
            }
            catch (IOException ex)
            {
                _error(ex);
            }
            catch (ObjectDisposedException ex)
            {
                _error(ex);
            }
        }

        private void EndWrite(IAsyncResult ar)
        {
            try
            {
                _destination.EndWrite(ar);
            }
            catch (IOException ex)
            {
                _error(ex);
            }
            catch (ObjectDisposedException ex)
            {
                _error(ex);
            }
            BeginRead();
        }
    }
}