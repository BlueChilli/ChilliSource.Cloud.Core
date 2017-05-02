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
        /// Convert object to a byte array.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToByteArray(this object value)
        {
            if (value == null) return null;
            if (value is String) return ((string)value).ToByteArray(new UTF8Encoding());
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, value);
            return ms.ToArray();
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

            if (typeof(T) == typeof(string)) return (T)(object)Encoding.UTF8.GetString(value);
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(value, 0, value.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return (T)obj;
        }
    }
}
