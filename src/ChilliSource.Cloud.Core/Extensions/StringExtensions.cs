﻿using ChilliSource.Cloud.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure.Pluralization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    //So doesn't clash with System.Web.Pages.StringExtensions
    /// <summary>
    /// Extension methods for string.
    /// </summary>
    public static class StringExtensions
    {
        #region Sentence formating

        /// <summary>
        /// Returns the plural form of a word if count is not 1.
        /// </summary>
        /// <param name="s">Word to pluralise.</param>
        /// <param name="count">Number of items that represent word.</param>
        /// <param name="outputCount">Optionally return word prefixed with coun.t</param>
        /// <returns>A string value in plural format.</returns>
        public static string ToPlural(this string s, int count = 0, bool outputCount = false)
        {
            if (count != 1)
                s = new EnglishPluralizationService().Pluralize(s);
            return (outputCount) ? String.Format("{0} {1}", count, s) : s;
        }
        #endregion

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
        /// Converts JSON string to object using custom JavaScript converter.
        /// </summary>
        /// <typeparam name="T">The type of the object to convert.</typeparam>
        /// <param name="source">The JSON string.</param>
        /// <param name="converter">The custom JavaScript converter.</param>
        /// <returns>A reference to the newly created object representing JSON string.</returns>
        public static T FromJson<T>(this string source, JsonSerializer converter)
        {
            if (source == null)
                return default(T);
            {
                using (var txtReader = new StringReader(source))
                using (var reader = new JsonTextReader(txtReader))
                {
                    return converter.Deserialize<T>(reader);
                }
            }
        }

        /// <summary>
        /// Converts JSON string to object.
        /// </summary>
        /// <typeparam name="T">The type of the object to convert.</typeparam>
        /// <param name="s">The JSON string.</param>
        /// <returns>A reference to the newly created object representing JSON string.</returns>
        public static T FromJson<T>(this string source, Formatting format = Formatting.Indented, IContractResolver resolver = null)
        {
            if (source == null)
                return default(T);

            if (resolver == null)
            {
                resolver = new DefaultContractResolver();
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(source, new JsonSerializerSettings()
            {
                ContractResolver = resolver,
                Formatting = format
            });
        }

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

        #region String based helper functions
        //Dynamic cannot be called in an extension method.
        /// <summary>
        /// Replaces {Placeholder} text in string with property values from the dynamic object.
        /// </summary>
        /// <param name="s">The specified string value.</param>
        /// <param name="transformWith">The dynamic object to replace.</param>
        /// <returns>A string value replaced by property values from dynamic object.</returns>
        public static string TransformWithDynamic(string s, dynamic transformWith)
        {
            var data = (transformWith as IDictionary<string, object>);
            if (data == null)
                throw new ArgumentException("transformWith must be convertable to IDictionary<string, object>");

            foreach (KeyValuePair<string, object> property in data)
            {
                s = s.Replace("{" + property.Key + "}", (property.Value ?? "").ToString());
                s = s.Replace("%7B" + property.Key + "%7D", (property.Value ?? "").ToString());
            }

            return s;
        }

        #endregion
    }
}
