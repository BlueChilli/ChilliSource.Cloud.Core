using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ChilliSource.Cloud.Core
{
    internal class PipedStreamReader : Stream, IDisposable
    {
        PipedStreamManager _pipe;
        IPipeBufferItem _currentData;
        int _currentDataPos;
        int _position;

        public PipedStreamReader(PipedStreamManager pipe)
        {
            _pipe = pipe;

            _currentData = null;
            _currentDataPos = 0;
            _position = 0;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            /* no need to flush */
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remainingCount = count;
            while (remainingCount > 0)
            {
                if (_currentData == null)
                {
                    _currentData = _pipe.ReceiveSync();
                    _currentDataPos = 0;

                    if (_currentData == null)
                    {
                        break;   // No more bytes will be available. Finished.                                  
                    }
                }

                var remainingBufferSize = _currentData.WrittenLength - _currentDataPos;
                var readCount = remainingCount > remainingBufferSize ? remainingBufferSize : remainingCount;

                _currentData.Read(_currentDataPos, buffer, offset, readCount);
                _currentDataPos += readCount;

                if (_currentData.WrittenLength == _currentDataPos)
                {
                    _currentData = null;
                    _currentDataPos = 0;
                }

                offset += readCount;
                remainingCount -= readCount;
            }

            var bytesRead = count - remainingCount;
            _position += bytesRead;

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var remainingCount = count;
            while (remainingCount > 0)
            {
                if (_currentData == null)
                {
                    _currentData = await _pipe.ReceiveAsync(cancellationToken);
                    _currentDataPos = 0;

                    if (_currentData == null)
                    {
                        break;   // No more bytes will be available. Finished.                                  
                    }
                }

                var remainingBufferSize = _currentData.WrittenLength - _currentDataPos;
                var readCount = remainingCount > remainingBufferSize ? remainingBufferSize : remainingCount;

                _currentData.Read(_currentDataPos, buffer, offset, readCount);
                _currentDataPos += readCount;

                if (_currentData.WrittenLength == _currentDataPos)
                {
                    _currentData = null;
                    _currentDataPos = 0;
                }

                offset += readCount;
                remainingCount -= readCount;
            }

            var bytesRead = count - remainingCount;
            _position += bytesRead;

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            base.Close();

            //Makes sure the pipe is closed (nothing else can be written)
            //Realeases any blocked writers
            _pipe.ClosePipe();
        }
    }
}
