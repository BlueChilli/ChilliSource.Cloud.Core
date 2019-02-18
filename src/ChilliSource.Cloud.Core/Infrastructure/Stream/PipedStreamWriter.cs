using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ChilliSource.Core.Extensions;

namespace ChilliSource.Cloud.Core
{
    internal class PipedStreamWriter : Stream, IDisposable
    {
        PipedStreamManager _pipe;
        long _length;
        bool _throwsFailedWrite;
        bool _autoFlush;
        IPipeBufferItem _currentBuffer;

        public PipedStreamWriter(PipedStreamManager pipe, bool throwsFailedWrite, bool autoFlush)
        {
            _pipe = pipe;
            _length = 0;
            _throwsFailedWrite = throwsFailedWrite;
            _autoFlush = autoFlush;
            _currentBuffer = null;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _pipe.RaiseOnWrite(count);

            bool? flushed = null;
            var blockSize = _pipe.BlockSize;
            var remainingCount = count;

            while (remainingCount > 0 && !_pipe.IsClosed())
            {
                if (_currentBuffer == null)
                {
                    _currentBuffer = _pipe.CreateNewBufferItem();
                }

                var remaningBufferSize = blockSize - _currentBuffer.WrittenLength;

                var writeCount = remainingCount > remaningBufferSize ? remaningBufferSize : remainingCount;
                _currentBuffer.Write(buffer, offset, writeCount);

                offset += writeCount;
                remainingCount -= writeCount;

                if (_currentBuffer.WrittenLength == blockSize)
                {
                    flushed = this.FlushInternalSync();
                    if (flushed == false)
                    {
                        break;
                    }
                }
            }

            Interlocked.Add(ref _length, count - remainingCount);
            var sent = remainingCount == 0 && flushed != false;
            if (sent && _autoFlush)
            {
                sent = this.FlushInternalSync();
            }

            AssertBufferWasSent(sent);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _pipe.RaiseOnWrite(count);

            bool? flushed = null;
            var blockSize = _pipe.BlockSize;
            var remainingCount = count;

            while (remainingCount > 0 && !_pipe.IsClosed())
            {
                if (_currentBuffer == null)
                {
                    _currentBuffer = _pipe.CreateNewBufferItem();
                }

                var remaningBufferSize = blockSize - _currentBuffer.WrittenLength;

                var writeCount = remainingCount > remaningBufferSize ? remaningBufferSize : remainingCount;
                _currentBuffer.Write(buffer, offset, writeCount);

                offset += writeCount;
                remainingCount -= writeCount;

                if (_currentBuffer.WrittenLength == blockSize)
                {
                    flushed = await this.FlushInternalAsync(cancellationToken).IgnoreContext();
                    if (flushed == false)
                    {
                        break;
                    }
                }
            }

            Interlocked.Add(ref _length, count - remainingCount);

            var sent = remainingCount == 0 && flushed != false;
            if (sent && _autoFlush)
            {
                sent = await this.FlushInternalAsync(cancellationToken).IgnoreContext();
            }

            AssertBufferWasSent(sent);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        private void AssertBufferWasSent(bool sent)
        {
            if (!sent)
            {
                if (!_pipe.IsClosed())
                {
                    _pipe.FaultPipe();
                }

                if (_throwsFailedWrite)
                {
                    throw new ApplicationException("Error sending to PipedStream.");
                }
            }
        }

        private async Task<bool> FlushInternalAsync(CancellationToken cancellationToken)
        {
            if (_currentBuffer == null)
                return true; //nothing to flush

            var sent = await _pipe.SendAsync(_currentBuffer, cancellationToken).IgnoreContext();
            _currentBuffer = null;

            return sent;
        }

        private bool FlushInternalSync()
        {
            if (_currentBuffer == null)
                return true; //nothing to flush

            var sent = _pipe.SendSync(_currentBuffer);
            _currentBuffer = null;

            return sent;
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            AssertBufferWasSent(await FlushInternalAsync(cancellationToken).IgnoreContext());
        }

        public override void Flush()
        {
            AssertBufferWasSent(FlushInternalSync());
        }

        public override long Length => _length;

        public override long Position
        {
            get
            {
                return _length;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            try
            {
                base.Close();

                this.Flush();
            }
            finally
            {
                //Makes sure the pipe is closed (nothing else can be written)
                //Realeases any blocked readers
                _pipe.ClosePipe();
            }
        }
    }
}
