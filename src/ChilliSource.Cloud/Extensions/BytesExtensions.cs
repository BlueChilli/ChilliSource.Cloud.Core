using ChilliSource.Cloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public static class BytesExtensions
    {
        /// <summary>
        /// Compresses object by using GZip or Deflate algorithm.
        /// </summary>
        /// <param name="value">The object to compress.</param>
        /// <param name="method">The algorithm to use.</param>
        /// <returns>A byte array representing compressed object.</returns>
        public static byte[] Compress(this byte[] bytes, CompressMethod method = CompressMethod.GZip)
        {
            using (var outputStream = new MemoryStream())
            {
                switch (method)
                {
                    case CompressMethod.GZip: return GZipCompress(bytes, outputStream);
                    case CompressMethod.Deflate: return DeflateCompress(bytes, outputStream);
                    default: return null;
                }
            }
        }

        private static byte[] GZipCompress(byte[] bytes, MemoryStream outputStream)
        {
            using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
            {
                zip.Write(bytes, 0, bytes.Length);
                zip.Close();
            }
            return outputStream.ToArray();
        }

        private static byte[] DeflateCompress(byte[] bytes, MemoryStream outputStream)
        {
            using (var zip = new DeflateStream(outputStream, CompressionMode.Compress))
            {
                zip.Write(bytes, 0, bytes.Length);
                zip.Close();
            }
            return outputStream.ToArray();
        }

        /// <summary>
        /// Decompresses object by using GZip or Deflate algorithm.
        /// </summary>
        /// <param name="value">The byte array representing object to decompress.</param>
        /// <param name="method">The algorithm to use.</param>
        /// <returns>A byte array representing decompressed object.</returns>
        public static byte[] Decompress(this byte[] value, CompressMethod method = CompressMethod.GZip)
        {
            if (value == null) return null;
            using (var inputStream = new MemoryStream(value))
            using (var outputStream = new MemoryStream())
            {
                switch (method)
                {
                    case CompressMethod.GZip: return GZipDecompress(inputStream, outputStream);
                    case CompressMethod.Deflate: return DeflateDecompress(inputStream, outputStream);
                    default: return null;
                }
            }
        }

        private static byte[] DeflateDecompress(MemoryStream inputStream, MemoryStream outputStream)
        {
            using (var zip = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                byte[] bytes = new byte[4096];
                int n;
                while ((n = zip.Read(bytes, 0, bytes.Length)) != 0)
                {
                    outputStream.Write(bytes, 0, n);
                }
                zip.Close();
            }
            return outputStream.ToArray();
        }

        private static byte[] GZipDecompress(MemoryStream inputStream, MemoryStream outputStream)
        {
            using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                byte[] bytes = new byte[4096];
                int n;
                while ((n = zip.Read(bytes, 0, bytes.Length)) != 0)
                {
                    outputStream.Write(bytes, 0, n);
                }
                zip.Close();
            }
            return outputStream.ToArray();
        }
    }

    /// <summary>
    /// Enumeration values of algorithm used for compression and decompression.
    /// </summary>
    public enum CompressMethod
    {
        /// <summary>
        /// GZip algorithm used for compression and decompression.
        /// </summary>
        GZip,
        /// <summary>
        /// Deflate algorithm used for compression and decompression.
        /// </summary>
        Deflate
    }
}
