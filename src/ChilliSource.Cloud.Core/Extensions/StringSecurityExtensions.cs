using System;
using System.ComponentModel;

namespace ChilliSource.Cloud.Core
{
    //So doesn't clash with System.Web.Pages.StringExtensions
    /// <summary>
    /// Extension methods for string.
    /// </summary>
    public static class StringSecurityExtensions
    {
        #region Security
        /// <summary>
        /// Computes the hash value for the specified string with salt using System.Security.Cryptography.SHA256Managed hash algorithms.
        /// </summary>
        /// <param name="s">The specified string value.</param>
        /// <param name="salt">The salt string used to compute hash value.</param>
        /// <returns>The computed hash string.</returns>
        public static string SaltedHash(this string s, string salt)
        {
            return EncryptionHelper.GenerateSaltedHash(s, salt);
        }

        /// <summary>
        /// Encrypts the given string using AES.
        /// </summary>
        /// <param name="s">The string to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        /// <returns>The encrypted string using AES.</returns>
        public static string AesEncrypt(this string s, string sharedSecret, string salt)
        {
            return EncryptionHelper.EncryptStringAes(s, sharedSecret, salt);
        }

        /// <summary>
        /// Decrypts the given string using AES.
        /// </summary>
        /// <param name="s">The string to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <returns>The decrypted string.</returns>
        public static string AesDecrypt(this string s, string sharedSecret, string salt)
        {
            return EncryptionHelper.DecryptStringAes(s, sharedSecret, salt);
        }
        #endregion
    }
}
