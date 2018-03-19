using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal class PipeBufferItem : IPipeBufferItem
    {
        byte[] _buffer;
        int _writtenLength;

        public PipeBufferItem(int blockSize)
        {
            _buffer = new byte[blockSize];
            _writtenLength = 0;
        }

        public int WrittenLength => _writtenLength;

        public void Read(int bufferItemOffset, byte[] buffer, int offset, int count)
        {
            if ((bufferItemOffset + count) > _writtenLength)
                throw new IndexOutOfRangeException("PipeBufferItem.Read");

            Buffer.BlockCopy(_buffer, bufferItemOffset, buffer, offset, count);
        }

        public void Reset()
        {
            _writtenLength = 0;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if ((_writtenLength + count) > _buffer.Length)
                throw new IndexOutOfRangeException("PipeBufferItem.Write");

            Buffer.BlockCopy(buffer, offset, _buffer, _writtenLength, count);
            _writtenLength += count;
        }
    }
}
