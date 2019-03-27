using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) with a shorter string value.
    /// </summary>
    [TypeConverter(typeof(ShortGuidTypeConverter))]
    public struct ShortGuid
    {
        #region Static

        /// <summary>
        /// A read-only instance of the ShortGuid class whose value is guaranteed to be all zeroes. 
        /// </summary>
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        #endregion

        #region Fields

        Guid _guid;
        string _value;

        #endregion

        #region Contructors

        /// <summary>
        /// Creates a ShortGuid from a base64 encoded string.
        /// </summary>
        /// <param name="value">The encoded guid as a base64 string.</param>
        public ShortGuid(string value)
        {
            if (value != null && (value.Length == 32 || value.Length == 36))
            {
                ShortGuid shortGuid = Guid.Parse(value).ToShortGuid();
                _value = shortGuid._value;
                _guid = shortGuid._guid;
            }
            else
            {
                _guid = Decode(value).GetValueOrDefault(Guid.Empty);
                _value = _guid == Guid.Empty ? Empty.Value : value;
            }
        }

        /// <summary>
        /// Creates a ShortGuid from a Guid.
        /// </summary>
        /// <param name="guid">The Guid to encode.</param>
        public ShortGuid(Guid guid)
        {
            _value = Encode(guid);
            _guid = guid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the underlying Guid.
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (value != _guid)
                {
                    _guid = value;
                    _value = Encode(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the underlying base64 encoded string.
        /// </summary>
        public string Value
        {
            get { return _value ?? ShortGuid.Empty._value; }
            set
            {
                if (value != _value)
                {
                    _guid = Decode(value).GetValueOrDefault(Guid.Empty);
                    _value = _guid == Guid.Empty ? Empty.Value : value;
                }
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns the base64 encoded GUID as a string.
        /// </summary>
        /// <returns>the base64 encoded string.</returns>
        public override string ToString()
        {
            return this.Value;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True when the instance and a specified Object represent the same type and value, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ShortGuid)
                return _guid.Equals(((ShortGuid)obj)._guid);
            if (obj is Guid)
                return _guid.Equals((Guid)obj);
            if (obj is string && !String.IsNullOrEmpty((string)obj))
                return _guid.Equals(((ShortGuid)obj)._guid);
            return false;
        }

        #endregion

        #region GetHashCode

        /// <summary>
        /// Returns the hash code for underlying GUID.
        /// </summary>
        /// <returns>A hash value.</returns>
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        #endregion

        #region NewGuid

        /// <summary>
        /// Initialises a new instance of the ShortGuid class
        /// </summary>
        /// <returns>A ChilliSource.Cloud.Core.Types.ShortGuid.</returns>
        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }

        #endregion

        #region Encode

        /// <summary>
        /// Creates a new instance of a Guid using the string value, then returns the base64 encoded version of the GUID.
        /// </summary>
        /// <param name="value">An actual Guid string (i.e. not a ShortGuid).</param>
        /// <returns>The base64 encoded string of the GUID.</returns>
        public static string Encode(string value)
        {
            Guid guid = new Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given Guid as a base64 string that is 22 characters long.
        /// </summary>
        /// <param name="guid">The Guid to encode.</param>
        /// <returns>A base64 encoded string.</returns>
        public static string Encode(Guid guid)
        {
            if (guid == Guid.Empty)
                return String.Empty;

            string encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        #endregion

        #region Decode

        /// <summary>
        /// Decodes the given base64 string.
        /// </summary>
        /// <param name="value">The base64 encoded string of a GUID.</param>
        /// <returns>A new GUID.</returns>
        public static Guid? Decode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            try
            {
                value = value
                    .Replace("_", "/")
                    .Replace("-", "+");
                byte[] buffer = Convert.FromBase64String(value + "==");
                return new Guid(buffer);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if both ShortGuids have the same underlying GUID value.
        /// </summary>
        /// <param name="x">The first ChilliSource.Cloud.Core.Types.ShortGuid.</param>
        /// <param name="y">The second ChilliSource.Cloud.Core.Types.ShortGuid.</param>
        /// <returns>True when both ShortGuids have the same underlying GUID value, otherwise false.</returns>
        public static bool operator ==(ShortGuid x, ShortGuid y)
        {
            if ((object)x == null) return (object)y == null;
            return x._guid == y._guid;
        }

        /// <summary>
        /// Determines if both ShortGuids do not have the same underlying GUID value.
        /// </summary>
        /// <param name="x">>The first ChilliSource.Cloud.Core.Types.ShortGuid.</param>
        /// <param name="y">The second ChilliSource.Cloud.Core.Types.ShortGuid.</param>
        /// <returns>True when ShortGuids do not have the same underlying GUID value, otherwise false.</returns>
        public static bool operator !=(ShortGuid x, ShortGuid y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the ShortGuid to it's equivalent string.
        /// </summary>
        /// <param name="shortGuid">The specified ChilliSource.Cloud.Core.Types.ShortGuid.</param>
        /// <returns>A string value equivalent to the specified ChilliSource.Cloud.Core.Types.ShortGuid.</returns>
        public static implicit operator string(ShortGuid shortGuid)
        {
            return shortGuid.Value;
        }

        /// <summary>
        /// Implicitly converts the ChilliSource.Cloud.Core.Types.ShortGuid to it's equivalent GUID value.
        /// </summary>
        /// <param name="shortGuid">The specified ChilliSource.Cloud.Core.Types.ShortGuid.</param>
        /// <returns>A System.Guid value equivalent to the specified ChilliSource.Cloud.Core.Types.ShortGuid.</returns>
        public static implicit operator Guid(ShortGuid shortGuid)
        {
            return shortGuid.Guid;
        }

        /// <summary>
        /// Implicitly converts the string to ChilliSource.Cloud.Core.Types.ShortGuid.
        /// </summary>
        /// <param name="shortGuid">The specified string value.</param>
        /// <returns>A ChilliSource.Cloud.Core.Types.ShortGuid equivalent to the specified string value.</returns>
        public static implicit operator ShortGuid(string shortGuid)
        {
            return new ShortGuid(shortGuid);
        }

        /// <summary>
        /// Implicitly converts the System.Guid to a ChilliSource.Cloud.Core.Types.ShortGuid.
        /// </summary>
        /// <param name="guid">The System.Guid value.</param>
        /// <returns>A ChilliSource.Cloud.Core.Types.ShortGuid equivalent to the specified System.Guid value.</returns>
        public static implicit operator ShortGuid(Guid guid)
        {
            return new ShortGuid(guid);
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for ChilliSource.Cloud.Core.Types.ShortGuid.
    /// </summary>
    public static class ShortGuidExtension
    {
        /// <summary>
        /// Converts System.Guid to ChilliSource.Cloud.Core.Types.ShortGuid.
        /// </summary>
        /// <param name="guid">The specified System.Guid.</param>
        /// <returns>A ChilliSource.Cloud.Core.Types.ShortGuid.</returns>
        public static ShortGuid ToShortGuid(this Guid guid)
        {
            return new ShortGuid(guid);
        }
    }

    public class ShortGuidTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string)
                    || sourceType == typeof(Guid)
                    || sourceType == typeof(Guid?)
                    || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string)
                    || destinationType == typeof(Guid)
                    || destinationType == typeof(Guid?)
                    || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return ShortGuid.Empty;

            if (value is String)
                return new ShortGuid((String)value);

            if (value is Guid)
                return new ShortGuid((Guid)value);

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var cast = (ShortGuid)value;

            if (destinationType == typeof(String))
                return cast.Value;

            if (destinationType == typeof(Guid))
                return cast.Guid;

            if (destinationType == typeof(Guid?))
                return (Guid?)cast.Guid;

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
