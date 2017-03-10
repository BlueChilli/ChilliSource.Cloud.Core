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

namespace ChilliSource.Cloud.Extensions
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
        /// Converts object to JSON string.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>JSON string representing the object.</returns>
        public static string ToJson(this object data, Formatting format = Formatting.Indented, IContractResolver resolver = null)
        {
            if (resolver == null)
            {
                resolver = new DefaultContractResolver();
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(data, format, new JsonSerializerSettings()
            {
                ContractResolver = resolver,
                Formatting = format
            });
        }

        /// <summary>
        /// Convert object to a byte array.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToByteArray(this object value)
        {
            if (value == null) return null;
            if (value is String) return ((String)value).ToByteArray();
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

        #region Helpers
        /// <summary>
        /// Returns a default object when source object is null.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="nullDefault">Default object.</param>
        /// <returns>The default object when source object is null, otherwise the source object.</returns>
        public static object DefaultTo(object source, object nullDefault)
        {
            return (source == null) ? nullDefault : source;
        }
        #endregion
    }
}
