using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    /// <summary>
    /// Base class to help encapsulate and modify the behaviour of a Stream
    /// </summary>
    public abstract class StreamModifier : Stream
    {
        private Stream _inner;

        protected void SetInnerStream(Stream inner)
        {
            _inner = inner;
        }

        public StreamModifier() { }

        protected virtual void CheckInitialize()
        {
            if (_inner == null)
                throw new ApplicationException("Object not initialized or disposed.");
        }

        public override bool CanRead
        {
            get { CheckInitialize(); return _inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { CheckInitialize(); return _inner.CanSeek; }
        }

        public override bool CanWrite
        {
            get { CheckInitialize(); return _inner.CanWrite; }
        }

        public override void Flush()
        {
            CheckInitialize(); _inner.Flush();
        }

        public override long Length
        {
            get { CheckInitialize(); return _inner.Length; }
        }

        public override long Position
        {
            get
            {
                CheckInitialize(); return _inner.Position;
            }
            set
            {
                CheckInitialize(); _inner.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckInitialize();
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CheckInitialize();
            _inner.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckInitialize();
            return _inner.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckInitialize();
            _inner.Write(buffer, offset, count);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            CheckInitialize();
            return _inner.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            CheckInitialize();
            return _inner.FlushAsync(cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckInitialize();
            return _inner.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckInitialize();
            return _inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Close()
        {
            base.Close();
            if (_inner != null) _inner.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _inner != null)
            {
                _inner.Dispose();
                _inner = null;
            }
        }
    }
}
