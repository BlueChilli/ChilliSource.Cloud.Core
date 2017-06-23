using System;
using System.ComponentModel;

namespace ChilliSource.Cloud.Core
{
    //So doesn't clash with System.Web.Pages.StringExtensions
    /// <summary>
    /// Extension methods for string.
    /// </summary>
    public static class StringExtensions
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

        #region Convert To and From

        /// <summary>
        /// Converts the specified string to the type of object.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="s">The specified string value.</param>
        /// <returns>The strongly typed object representing the converted text.</returns>
        public static T To<T>(this string s)
        {
            var type = typeof(T);
            if (String.IsNullOrEmpty(s))
            {
                return default(T);
            }

            Type valueType;
            if (IsNullableType(type, out valueType))
            {
                return (T)ToNullableValueType(valueType, s);
            }

            return (T)Convert.ChangeType(s, type);
        }

        private static bool IsNullableType(Type theType, out Type valueType)
        {
            valueType = null;
            if (theType.IsGenericType &&
            theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var args = theType.GetGenericArguments();
                if (args.Length > 0)
                    valueType = args[0];
            }

            return valueType != null;
        }

        //(e.g ValueType = int)
        // returns Nullable<int>
        private static object ToNullableValueType(Type valueType, string s)
        {
            if (!String.IsNullOrEmpty(s))
            {
                return TypeDescriptor.GetConverter(valueType).ConvertFrom(s);
            }
            return null;
        }

        /// <summary>
        /// Converts the specified string to the type of System.Nullable&lt;T&gt; generic type.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="s">The specified string value.</param>
        /// <returns>The System.Nullable&lt;T&gt; representing the converted text.</returns>
        public static Nullable<T> ToNullable<T>(this string s) where T : struct
        {
            return (Nullable<T>)ToNullableValueType(typeof(T), s);
        }

        #endregion
    }
}
