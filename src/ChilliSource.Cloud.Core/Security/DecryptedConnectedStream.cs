using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Creates a decrypted System.IO.Stream that encapsulates a regular Stream.
    /// This is a read-only Stream. All read data gets decrypted when reading.
    /// </summary>
    internal class DecryptedConnectedStream
    {
        public static Stream Create(Stream encryptedStream, string sharedSecret, string salt, bool leaveOpen)
        {
            Action<Stream> onDisposingAction = (s) =>
            {
                s?.Dispose();
                if (!leaveOpen)
                {
                    encryptedStream?.Dispose();
                }
            };

            return ReadOnlyStreamWrapper.Create(async () => await CreateAsync(encryptedStream, sharedSecret, salt).IgnoreContext(), onDisposingAction: onDisposingAction);
        }

        private static async Task<Stream> CreateAsync(Stream encryptedStream, string sharedSecret, string salt)
        {
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            ICryptoTransform decryptor = null;
            using (var aesAlg = new RijndaelManaged())
            using (var key = new Rfc2898DeriveBytes(sharedSecret, saltBytes))
            {
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Get the initialization vector from the encrypted stream
                aesAlg.IV = await ReadInitVectorIVAsync(encryptedStream)
                                .IgnoreContext();

                decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            }

            return new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);
        }

        private static async Task<byte[]> ReadInitVectorIVAsync(Stream s)
        {
            var rawLength = new byte[sizeof(int)];
            if ((await s.ReadAsync(rawLength, 0, rawLength.Length).IgnoreContext()) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted init vector");

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if ((await s.ReadAsync(buffer, 0, buffer.Length).IgnoreContext()) != buffer.Length)
                throw new SystemException("Did not read init vector properly");

            return buffer;
        }
    }
}
