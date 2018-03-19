using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ChilliSource.Cloud.Core
{
    public class PipedStreamManager
    {
        PipedStreamOptions _options;
        BufferBlock<IPipeBufferItem> _pipe;
        bool _closed;
        PipeBufferFactory _buffer;

        int _writersCreated;
        int _readersCreated;
        Lazy<PipedMultiplexer> _multiplexer;

        public PipedStreamManager()
            : this(new PipedStreamOptions())
        {
        }

        public PipedStreamManager(PipedStreamOptions options)
        {
            _options = options.Clone();
            _options.EnsureValidity();

            _writersCreated = 0;
            _readersCreated = 0;
            _multiplexer = _options.Multiplexed ? new Lazy<PipedMultiplexer>(CreateMultiplexer, LazyThreadSafetyMode.ExecutionAndPublication) : null;

            _buffer = new PipeBufferFactory(_options.BlockSize, _options.MaxBlocks + 2);

            _pipe = new BufferBlock<IPipeBufferItem>(new DataflowBlockOptions()
            {
                EnsureOrdered = true,
                BoundedCapacity = _options.MaxBlocks
            });
            _closed = false;
        }

        internal int BlockSize { get { return _options.BlockSize; } }

        public bool Multiplexed { get { return _options.Multiplexed; } }

        private PipedMultiplexer CreateMultiplexer()
        {
            return new PipedMultiplexer(new PipedStreamReader(this), _buffer.BlockSize);
        }

        /// <summary>
        /// The caller must dispose the stream after writing.
        /// </summary>
        /// <param name="throwsFailedWrite"></param>
        /// <returns></returns>
        public Stream CreateWriter(bool throwsFailedWrite)
        {
            if (Interlocked.Increment(ref _writersCreated) > 1)
                throw new ApplicationException("Only one writer is supported.");

            return new PipedStreamWriter(this, throwsFailedWrite);
        }

        /// <summary>
        /// The caller must dispose the stream after reading.
        /// </summary>
        /// <returns></returns>
        public Stream CreateReader()
        {
            if (Interlocked.Increment(ref _readersCreated) > 1 && !Multiplexed)
                throw new ApplicationException("Only one reader is supported when the stream is not multiplexed.");

            if (Multiplexed)
            {
                return _multiplexer.Value.CreateReader();
            }
            else
            {
                return new PipedStreamReader(this);
            }
        }

        public void ClosePipe()
        {
            _pipe.Complete();
            _closed = true;
        }

        internal bool IsClosed()
        {
            return _closed;
        }

        internal IPipeBufferItem CreateNewBufferItem()
        {
            return _buffer.CreateNewBufferItem();
        }

        internal bool SendSync(IPipeBufferItem item)
        {
            var posted = _pipe.Post(item);
            if (posted)
                return true;

            // _pipe.Post doesn't allow for postponement. Using Async version when posting fails.
            return Task.Run(() => SendAsync(item, CancellationToken.None)).GetAwaiter().GetResult();
        }

        private static CancellationTokenSource CreateCancellationTokenSource(TimeSpan? delay)
        {
            if (delay == null)
                return CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);

            return new CancellationTokenSource(delay.Value);
        }

        internal async Task<bool> SendAsync(IPipeBufferItem item, CancellationToken cancellationToken)
        {
            bool sent = false;
            using (var ct = CreateCancellationTokenSource(_options.WriteTimeout))
            using (var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct.Token, cancellationToken))
            {
                sent = await _pipe.SendAsync(item, linkedCt.Token);
            }

            return sent;
        }

        internal IPipeBufferItem ReceiveSync()
        {
            IPipeBufferItem item;
            var received = _pipe.TryReceive(out item);
            if (received)
                return item;

            // TryReceive doesn't allow for postponement. Using Async version when receive fails.            
            return Task.Run(() => ReceiveAsync(CancellationToken.None)).GetAwaiter().GetResult();
        }

        internal async Task<IPipeBufferItem> ReceiveAsync(CancellationToken cancellationToken)
        {
            IPipeBufferItem item = null;
            using (var ct = CreateCancellationTokenSource(_options.ReadTimeout))
            using (var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct.Token, cancellationToken))
            {
                if (await _pipe.OutputAvailableAsync(linkedCt.Token))
                {
                    item = await _pipe.ReceiveAsync(linkedCt.Token);
                }
            }

            return item;
        }
    }
}
