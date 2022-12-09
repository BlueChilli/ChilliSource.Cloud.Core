using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extension methods for System.Object.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts object to System.Nullable&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">The type of object to convert.</typeparam>
        /// <param name="o">Object to convert.</param>
        /// <returns>A System.Nullable&lt;T&gt;.</returns>
        public static T? ToNullable<T>(this object o) where T : struct
        {
            if (o == null) return new T?();
            if (Convert.IsDBNull(o)) return new T?();
            if (o is T) return (T)o;
            return (T)(TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(o));
        }

        /// <summary>
        /// Converts object to System.Dynamic.ExpandoObject.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>An System.Dynamic.ExpandoObject.</returns>
        public static dynamic ToDynamic(this object value)
        {
            if (value is ExpandoObject)
            {
                dynamic dynamic = (value as ExpandoObject);
                return dynamic;
            }

            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        /// <summary>
        /// Convert object to a byte array.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToByteArray(this object value)
        {
            if (value == null) return null;
            if (value is String) return ((string)value).ToByteArray(new UTF8Encoding());
            if (value is Guid) return ((Guid)value).ToByteArray();

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, value);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts byte array to a strongly typed object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="value">A byte array.</param>
        /// <returns>A strongly typed object.</returns>
        public static T To<T>(this byte[] value)
        {
            if (value == null || value.Length == 0)
                return default(T);

            if (typeof(T) == typeof(string)) return (T)(object)value.ToString(new UTF8Encoding());

            if (typeof(T) == typeof(Guid)) return (T)(object)new Guid(value);

            if (typeof(T) == typeof(Guid?)) return (T)(object)new Guid(value);

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                ms.Write(value, 0, value.Length);
                ms.Seek(0, SeekOrigin.Begin);
                var obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

        /// <summary>
        /// Encode a value using Base64 and substituting any unsafe characters
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns>Encoded value which can be decoded with UrlSafeDecode</returns>
        public static string UrlSafeEncode(this object value)
        {
            if (value == null) return null;

            var encoded = Convert.ToBase64String(value.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            encoded = encoded.TrimEnd('=');
            return encoded;
        }

        /// <summary>
        /// Decode a value encoded with UrlSafeEncode
        /// </summary>
        /// <typeparam name="T">Type to decode as</typeparam>
        /// <param name="value">Encoded value</param>
        /// <returns>Decoded value</returns>
        public static T UrlSafeDecode<T>(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return default(T);

            try
            {
                value = value
                    .Replace("_", "/")
                    .Replace("-", "+");
                var padding = value.Length % 4;
                if (padding > 0)
                    value += padding == 3 ? "=" : "==";

                byte[] buffer = Convert.FromBase64String(value);
                return buffer.To<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }
}
