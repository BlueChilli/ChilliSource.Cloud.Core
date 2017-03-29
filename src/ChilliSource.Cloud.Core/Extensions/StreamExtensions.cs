using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extension methods for System.IO.Stream.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts System.IO.Stream to byte array.
        /// </summary>
        /// <param name="s">The System.IO.Stream</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReadToByteArray(this Stream s)
        {
            int? streamLength = null;
            try
            {
                streamLength = (int)s.Length;
            }
            catch {/* noop */ }

            var bufferSize = Math.Min(32 * 1024, streamLength ?? 32 * 1024);

            using (var memoryStream = new MemoryStream(streamLength ?? bufferSize))
            {
                s.CopyTo(memoryStream, bufferSize);
                return memoryStream.ToArray();
            }
        }
    }
}
