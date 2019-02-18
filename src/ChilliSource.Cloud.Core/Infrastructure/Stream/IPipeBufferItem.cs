using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal interface IPipeBufferItem
    {
        int WrittenLength { get; }

        void Write(byte[] buffer, int offset, int count);
        void Read(int bufferItemOffset, byte[] buffer, int offset, int count);
    }
}
