using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Creates a wrapper around a stream so that it can only be read. All write operations will throw an exception.
    /// </summary>
    public static class ReadOnlyStreamWrapper
    {
        /// <summary>
        /// Creates an instance of a read-only stream.
        /// </summary>
        /// <param name="innerStream">A readable stream.</param>
        /// <param name="length">(optional) The stream length.</param>
        /// <param name="onDisposingAction">(optional) An action to be executed when the stream gets disposed.</param>
        /// <returns>A read-only stream</returns>
        [Obsolete("Use the override with [Action<Stream> onDisposingAction] instead.")]
        public static Stream Create(Stream innerStream, long? length = null, Action onDisposingAction = null)
        {
            return new ReadOnlyStreamWrapperImpl(innerStream, length, (Stream s) => onDisposingAction?.Invoke());
        }

        /// <summary>
        /// Creates an instance of a read-only stream.
        /// </summary>
        /// <param name="innerStream">A readable stream.</param>
        /// /// <param name="onDisposingAction">An action to be executed when the stream gets disposed.</param>
        /// <param name="length">(optional) The stream length.</param>
        /// <returns>A read-only stream</returns>
        public static Stream Create(Stream innerStream, Action<Stream> onDisposingAction, long? length = null)
        {
            return new ReadOnlyStreamWrapperImpl(innerStream, length, onDisposingAction);
        }

        /// <summary>
        /// Creates an instance of a read-only stream.
        /// </summary>
        /// <param name="asyncProvider">A async factory for a readable stream.</param>
        /// /// <param name="onDisposingAction">An action to be executed when the stream gets disposed.</param>
        /// <param name="length">(optional) The stream length.</param>
        /// <returns>A read-only stream</returns>
        public static Stream Create(Func<Task<Stream>> asyncProvider, Action<Stream> onDisposingAction, long? length = null)
        {
            return new ReadOnlyStreamWrapperAsyncImpl(asyncProvider, length, onDisposingAction);
        }
    }

    internal class ReadOnlyStreamWrapperImpl : Stream
    {
        Stream _innerStream;
        long? _length;
        Action<Stream> _onDisposingAction = null;

        public ReadOnlyStreamWrapperImpl(Stream innerStream, long? length, Action<Stream> onDisposingAction)
        {
            _innerStream = innerStream;
            _length = length;
            _onDisposingAction = onDisposingAction;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length ?? throw new NotSupportedException("Length is not defined.");

        public override long Position { get => _innerStream.Position; set => throw new NotSupportedException(); }

        public override void Flush() { /* noop */}

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
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

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
            base.Dispose(disposing);

            if (disposing)
            {
                _onDisposingAction?.Invoke(_innerStream);
                _onDisposingAction = null;

                _innerStream = null;
            }
        }
    }

    internal class ReadOnlyStreamWrapperAsyncImpl : Stream
    {
        Func<Task<Stream>> _streamProvider;
        Stream _innerStream;
        long? _length;
        Action<Stream> _onDisposingAction;

        public ReadOnlyStreamWrapperAsyncImpl(Func<Task<Stream>> streamProvider, long? length, Action<Stream> onDisposingAction)
        {
            _streamProvider = streamProvider;
            _innerStream = null;
            _length = length;
            _onDisposingAction = onDisposingAction;
        }

        private async Task<Stream> GetStreamAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException("Stream");

            if (_innerStream == null)
            {
                _innerStream = await _streamProvider().IgnoreContext();
            }

            return _innerStream;
        }

        private Stream GetStream()
        {
            if (_disposed)
                throw new ObjectDisposedException("Stream");

            if (_innerStream == null)
            {
                _innerStream = TaskHelper.GetResultSafeSync(_streamProvider);
            }

            return _innerStream;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length ?? throw new NotSupportedException("Length is not defined.");

        public override long Position { get => GetStream().Position; set => throw new NotSupportedException(); }

        public override void Flush() { /* noop */}

        public override int Read(byte[] buffer, int offset, int count)
        {
            return GetStream().Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await (await GetStreamAsync().IgnoreContext()).ReadAsync(buffer, offset, count, cancellationToken).IgnoreContext();
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

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
            base.Dispose(disposing);

            if (disposing)
            {
                _onDisposingAction?.Invoke(_innerStream);
                _onDisposingAction = null;

                _innerStream = null;
                _streamProvider = null;
            }
        }
    }
}
