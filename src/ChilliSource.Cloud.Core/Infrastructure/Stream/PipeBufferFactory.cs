using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal class PipeBufferFactory
    {
        int _backingCapacity;
        PipeBufferItem[] _backingBuffers;
        int _latestBlockIndex;

        public PipeBufferFactory(int blockSize, int backingCapacity)
        {
            this.BlockSize = blockSize;
            _backingCapacity = backingCapacity;
            _backingBuffers = new PipeBufferItem[_backingCapacity];
            _latestBlockIndex = -1;
        }

        public int BlockSize { get; private set; }

        public IPipeBufferItem CreateNewBufferItem()
        {
            var index = ++_latestBlockIndex;
            index = index % _backingCapacity;

            var buffer = _backingBuffers[index];
            if (buffer == null)
            {
                buffer = _backingBuffers[index] = new PipeBufferItem(BlockSize);
            }
            else
            {
                buffer.Reset();
            }

            return buffer;
        }
    }
}
