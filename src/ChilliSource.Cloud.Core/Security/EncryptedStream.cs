using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Contains an encrypted System.IO.Stream that encapsulates a regular Stream.
    /// This is a read-only Stream. All data gets encrypted when reading.
    /// </summary>
    public class EncryptedStream : StreamModifier
    {
        private EncryptedStream() { }
        private async Task InitAsync(Stream originalStream, string sharedSecret, string salt)
        {
            originalStream.Position = 0;
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            var memStream = new MemoryStream();
            // generates the key from the shared secret and the salt
            var key = new Rfc2898DeriveBytes(sharedSecret, saltBytes);

            // Creates a RijndaelManaged object
            using (var aesAlg = new RijndaelManaged())
            {
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Creates an encryptor to perform the stream transform.
                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    // prepends the IV
                    memStream.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int)); //async won't matter here (MemoryStream)
                    memStream.Write(aesAlg.IV, 0, aesAlg.IV.Length); //async won't matter here (MemoryStream)

                    var _CryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
                    await originalStream.CopyToAsync(_CryptoStream)
                          .IgnoreContext();

                    _CryptoStream.Flush(); //async won't matter here (MemoryStream)
                    if (!_CryptoStream.HasFlushedFinalBlock)
                        _CryptoStream.FlushFinalBlock();
                }
            }

            memStream.Position = 0;
            this.SetInnerStream(memStream);
        }

        public static EncryptedStream Create(Stream originalStream, string sharedSecret, string salt)
        {
            var instance = new EncryptedStream();
            TaskHelper.WaitSafeSync(() => instance.InitAsync(originalStream, sharedSecret, salt));

            return instance;
        }

        public async static Task<EncryptedStream> CreateAsync(Stream originalStream, string sharedSecret, string salt)
        {
            var instance = new EncryptedStream();
            await instance.InitAsync(originalStream, sharedSecret, salt)
                  .IgnoreContext();

            return instance;
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
        /// EncryptedStream.Write: Method not supported.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new ApplicationException("EncryptedStream.Write: Method not supported.");
        }
    }
}
