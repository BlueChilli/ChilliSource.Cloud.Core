using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using System.Threading;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Contains a decrypted System.IO.Stream that encapsulates a regular Stream.
    /// This is a read-only Stream. All data gets decrypted when reading.
    /// </summary>
    public class DecryptedStream : StreamModifier
    {
        private DecryptedStream() { }
        private async Task InitAsync(Stream encryptedStream, string sharedSecret, string salt, int contentLength, CancellationToken cancellationToken)
        {
            MemoryStream inputStream;

            if (encryptedStream is MemoryStream)
                inputStream = encryptedStream as MemoryStream;
            else
            {
                inputStream = new MemoryStream(contentLength);
                await encryptedStream.CopyToAsync(inputStream, Math.Min(80 * 1024, contentLength), cancellationToken)
                     .IgnoreContext();
            }

            var memStream = new MemoryStream(contentLength);

            using (inputStream)
            {
                inputStream.Position = 0;
                var saltBytes = Encoding.UTF8.GetBytes(salt);

                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, saltBytes);
                // Create a RijndaelManaged object
                // with the specified key and IV.
                using (var aesAlg = new RijndaelManaged())
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadInitVectorIV(inputStream);
                    var initPosition = inputStream.Position;

                    try
                    {
                        // Create a decryptor to perform the stream transform.
                        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                        using (CryptoStream _CryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                        {
                            await _CryptoStream.CopyToAsync(memStream, Math.Min(80 * 1024, contentLength), cancellationToken)
                                  .IgnoreContext(); ;
                        }
                    }
                    catch
                    {
                        //If error occurs, tries to read without padding;
                        inputStream.Position = initPosition;
                        aesAlg.Padding = PaddingMode.None;

                        // Create a decryptor to perform the stream transform.
                        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                        using (CryptoStream _CryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                        {
                            await _CryptoStream.CopyToAsync(memStream, Math.Min(80 * 1024, contentLength), cancellationToken)
                                  .IgnoreContext();
                        }
                    }
                }
            }

            memStream.Position = 0;
            this.SetInnerStream(memStream);
        }

        public static DecryptedStream Create(Stream encryptedStream, string sharedSecret, string salt, int contentLength)
        {
            var instance = new DecryptedStream();
            TaskHelper.WaitSafeSync(() => instance.InitAsync(encryptedStream, sharedSecret, salt, contentLength, CancellationToken.None));

            return instance;
        }

        public async static Task<DecryptedStream> CreateAsync(Stream encryptedStream, string sharedSecret, string salt, int contentLength, CancellationToken cancellationToken = default(CancellationToken))
        {
            var instance = new DecryptedStream();
            await instance.InitAsync(encryptedStream, sharedSecret, salt, contentLength, cancellationToken)
                  .IgnoreContext();

            return instance;
        }

        private static byte[] ReadInitVectorIV(MemoryStream s)
        {
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted init vector");

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read init vector properly");

            return buffer;
        }

        /// <summary>
        /// Clients of this class can only read.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// DecryptedStream.Write: Method not supported.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new ApplicationException("DecryptedStream.Write: Method not supported.");
        }
    }
}
