using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// Computes the hash value for the specified string with salt using System.Security.Cryptography.SHA256Managed hash algorithms.
        /// </summary>
        /// <param name="input">The specified string value.</param>
        /// <param name="salt">The salt string used to compute hash value.</param>
        /// <returns>The computed hash string.</returns>
        public static string GenerateSaltedHash(string input, string salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();
            UTF8Encoding enc = new UTF8Encoding();
            
            var inputBytes = enc.GetBytes(input).Concat(enc.GetBytes(salt)).ToArray();

            return BitConverter.ToString(algorithm.ComputeHash(inputBytes));
        }

        /// <summary>
        /// Generates an MD5 hash of the given string which is Url safe
        /// </summary>
        /// <param name="input">The input to compute the hash string for.</param>
        /// <returns>The computed hash string.</returns>
        /// <remarks>Source: http://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx </remarks>
        public static string GetMd5Hash(params string[] input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(String.Join("", input)));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        #region Encrypt / Decrypt String AES

        /// <summary>
        /// Encrypts the given string using AES. The string can be decrypted using DecryptStringAES(). The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        /// <returns>The encrypted string using AES.</returns>
        public static string EncryptStringAes(string plainText, string sharedSecret, string salt)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            var saltBytes = Encoding.UTF8.GetBytes(salt);

            // generate the key from the shared secret and the salt
            var key = new Rfc2898DeriveBytes(sharedSecret, saltBytes);

            // Create a RijndaelManaged object
            using (var aesAlg = new RijndaelManaged())
            {
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray()).Replace('+', '-').Replace('/', '_').Replace('=', '.');
                }
            }
        }

        /// <summary>
        /// Decrypts the given string. Assumes the string was encrypted using EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <returns>The decrypted string.</returns>
        public static string DecryptStringAes(string cipherText, string sharedSecret, string salt)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            var saltBytes = Encoding.UTF8.GetBytes(salt);

            // generate the key from the shared secret and the salt
            var key = new Rfc2898DeriveBytes(sharedSecret, saltBytes);

            // Create the streams used for decryption.                
            byte[] bytes = Convert.FromBase64String(cipherText.Replace('-', '+').Replace('_', '/').Replace('.', '='));
            using (var msDecrypt = new MemoryStream(bytes))
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                using (var aesAlg = new RijndaelManaged())
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted byte array");

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read byte array properly");

            return buffer;
        }
        #endregion

        public static Stream CreateConnectedStream(EncryptionMode mode, Stream sourceStream, string sharedSecret, string salt, bool leaveOpen)
        {
            return mode == EncryptionMode.Encrypt ? EncryptedConnectedStream.Create(sourceStream, sharedSecret, salt, leaveOpen)
                            : DecryptedConnectedStream.Create(sourceStream, sharedSecret, salt, leaveOpen);
        }
    }

    public enum EncryptionMode
    {
        Encrypt = 1,
        Decrypt
    }
}
