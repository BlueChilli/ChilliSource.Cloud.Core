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
    internal class EncryptedConnectedStream
    {
        public static Stream Create(Stream originalStream, string sharedSecret, string salt, bool leaveOpen)
        {
            var pipeOptions = new PipedStreamOptions() { WriteTimeout = TimeSpan.FromMinutes(10) };
            var pipe = new PipedStreamManager(pipeOptions);

            var saltBytes = Encoding.UTF8.GetBytes(salt);

            var encryptorTask = Task.Run(async () =>
            {
                try
                {
                    using (var writerStream = pipe.CreateWriter(throwsFailedWrite: true))
                    using (var aesAlg = new RijndaelManaged())
                    using (var key = new Rfc2898DeriveBytes(sharedSecret, saltBytes))
                    {
                        try
                        {
                            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                            // prepends the IV
                            await writerStream.WriteAsync(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int))
                                .IgnoreContext();
                            await writerStream.WriteAsync(aesAlg.IV, 0, aesAlg.IV.Length)
                                .IgnoreContext();

                            // Creates an encryptor to perform the stream transform.
                            using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                            using (var cryptoStream = new CryptoStream(writerStream, encryptor, CryptoStreamMode.Write))
                            {
                                await originalStream.CopyToAsync(cryptoStream, bufferSize: pipeOptions.BlockSize)
                                      .IgnoreContext();

                                await cryptoStream.FlushAsync()
                                      .IgnoreContext();

                                await writerStream.FlushAsync()
                                        .IgnoreContext();

                                if (!cryptoStream.HasFlushedFinalBlock)
                                    cryptoStream.FlushFinalBlock();

                                await writerStream.FlushAsync()
                                        .IgnoreContext();
                            }
                        }
                        catch (Exception ex)
                        {
                            pipe.FaultPipe(ex);
                        }
                    }
                }
                finally
                {
                    if (!leaveOpen)
                        originalStream.Dispose();
                }
            });

            return ReadOnlyStreamWrapper.Create(pipe.CreateReader(), onDisposingAction: (Stream s) => s?.Dispose());
        }
    }
}
