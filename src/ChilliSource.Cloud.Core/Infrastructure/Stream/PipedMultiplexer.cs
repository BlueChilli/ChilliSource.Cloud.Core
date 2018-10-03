using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal class PipedMultiplexer
    {
        private ConcurrentBag<Stream> _writers;
        private PipedStreamReader _reader;
        private PipedStreamOptions _optionsTemplate;
        private Task _task;

        public PipedMultiplexer(PipedStreamReader reader, PipedStreamOptions options)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _writers = new ConcurrentBag<Stream>();
            _reader = reader;
            _optionsTemplate = options.Clone();
            _optionsTemplate.Multiplexed = false;

            _task = Task.Run(async () => await MultiplexerTask());
        }

        public Stream CreateReader()
        {
            var newPipe = new PipedStreamManager(_optionsTemplate);
            _writers.Add(newPipe.CreateWriter(throwsFailedWrite: false));

            return newPipe.CreateReader();
        }

        private async Task MultiplexerTask()
        {
            var bufferSize = _optionsTemplate.BlockSize;
            var buffer = new byte[bufferSize];
            int read = 0;

            try
            {
                using (_reader)
                {
                    while ((read = await _reader.ReadAsync(buffer, 0, bufferSize)) > 0)
                    {
                        await Task.WhenAll(WriteToAllAsync(buffer, 0, read));
                    }

                    _reader.Close();
                }
            }
            finally
            {
                foreach (var writer in _writers)
                {
                    writer.Dispose();
                }
            }
        }

        private IEnumerable<Task> WriteToAllAsync(byte[] buffer, int offset, int count)
        {
            foreach (var writer in _writers)
            {
                yield return writer.WriteAsync(buffer, offset, count);
            }
        }
    }
}
