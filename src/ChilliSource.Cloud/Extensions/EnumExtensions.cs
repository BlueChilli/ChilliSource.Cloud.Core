using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace ChilliSource.Cloud.Extensions
{
    /// <summary>
    /// Extension methods for System.Enum type.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description attribute of the enumeration value.
        /// </summary>
        /// <param name="e">The specified enumeration value.</param>
        /// <returns>The description attribute of the enumeration value.</returns>
        public static string GetDescription(this Enum e)
        {
            return GetEnumDescription((dynamic)e);
        }

        /// <summary>
        /// Converts the value of the specified enumeration to a 32-bit signed integral string.
        /// </summary>
        /// <param name="e">The specified enumeration value.</param>
        /// <returns>The integral string value of the specified enumeration.</returns>
        public static string ToValueString(this Enum e)
        {
            return Convert.ToInt32(e).ToString();
        }

        /// <summary>
        /// Checks whether the specified enumeration value is in the System.Collections.Generic.IEnumerable%lt;T&gt; list.
        /// </summary>
        /// <typeparam name="T">The type of object to check.</typeparam>
        /// <param name="e">The specified enumeration.</param>
        /// <param name="list">System.Collections.Generic.IEnumerable%lt;T&gt; list.</param>
        /// <returns>True when the specified enumeration value is in the System.Collections.Generic.IEnumerable%lt;T&gt; list, otherwise false.</returns>
        public static bool IsIn<T>(this T e, IEnumerable<T> list) where T : struct, IConvertible, IFormattable
        {
            Type t = e.GetType();
            var enumValue = e as Enum;

            bool isFlags = t.GetCustomAttributes<FlagsAttribute>().Any();

            foreach (T item in list)
            {
                if (e.Equals(item) || (isFlags && enumValue.HasFlag(item as Enum))) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the specified enumeration value is in the enumeration parameter list.
        /// </summary>
        /// <typeparam name="T">The type of object to check.</typeparam>
        /// <param name="e">The specified enumeration.</param>
        /// <param name="list">The enumeration parameter list</param>
        /// <returns>True when the specified enumeration value is in the enumeration parameter list, otherwise false.</returns>
        public static bool IsIn<T>(this T e, params T[] list) where T : struct, IConvertible, IFormattable
        {
            return IsIn<T>(e, (list as IEnumerable<T>) ?? Enumerable.Empty<T>());
        }

        /// <summary>
        /// Return the next enum in sequence
        /// </summary>
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        /// <summary>
        /// Return the previous enum in sequence
        /// </summary>
        public static T Previous<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) - 1;
            return (j < 0) ? Arr.Last() : Arr[j];
        }


        /// <summary>
        /// Appends flag value to the specified enumeration value.
        /// </summary>
        /// <typeparam name="T">Type of object to append flag value.</typeparam>
        /// <param name="type">The specified enumeration value.</param>
        /// <param name="enumFlag">Enumeration flag to append.</param>
        /// <returns>The object with flag value appended.</returns>
        public static T AddFlag<T>(this Enum type, T enumFlag)
        {
            try
            {
                return (T)(object)((int)(object)type | (int)(object)enumFlag);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Could not append flag value {0} to enum {1}", enumFlag, typeof(T).Name), ex);
            }
        }

        /// <summary>
        /// Removes flag value form the specified enumeration value.
        /// </summary>
        /// <typeparam name="T">Type of object to remove flag value.</typeparam>
        /// <param name="type">The specified enumeration value.</param>
        /// <param name="enumFlag">Enumeration flag to remove.</param>
        /// <returns>The object with flag value removed.</returns>
        public static T RemoveFlag<T>(this Enum type, T enumFlag)
        {
            try
            {
                return (T)(object)((int)(object)type & ~(int)(object)enumFlag);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Could not remove flag value {0} from enum {1}", enumFlag, typeof(T).Name), ex);
            }
        }

        /// <summary>
        /// Checks whether the specified enumeration value is same as the string value.
        /// </summary>
        /// <param name="e">The specified enumeration value.</param>
        /// <param name="value">The string value to match.</param>
        /// <returns>True when the specified enumeration value is same as the string value, otherwise false.</returns>
        public static bool Match(this Enum e, string value)
        {
            return (Enum.Parse(e.GetType(), value) == e);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
        /// </summary>
        /// <typeparam name="T">Type of the object converted.</typeparam>
        /// <param name="e">The specified enumeration value.</param>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <returns>An object of type enumType whose value is represented by value.</returns>
        public static T Parse<T>(this Enum e, string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Converts enumeration type with Flags attribute to System.Collections.Generic.List&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">Type of the object converted.</typeparam>
        /// <param name="e">>The specified enumeration type.</param>
        /// <returns>A System.Collections.Generic.List&lt;T&gt;.</returns>
        public static List<T> ToFlagsList<T>(this T e) where T : struct, IConvertible, IFormattable //The compiler doesnt allow [where T: System.Enum]
        {
            var type = typeof(T);
            if (!type.IsEnum || !type.GetCustomAttributes<FlagsAttribute>().Any())
                throw new ArgumentException("The argument must be a System.Enum with the Flags attribute");

            var enumValue = e as Enum;
            return Enum.GetValues(type).Cast<T>().Where(flag => enumValue.HasFlag(flag as Enum)).ToList();
        }

        public static T ToFlags<T>(this List<T> flagsList) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new NotSupportedException(string.Format("{0} is not an Enum", typeof(T).Name));

            T obj1 = default(T);
            foreach (T obj2 in flagsList)
                obj1 = (T)(ValueType)((int)(ValueType)obj1 | (int)(ValueType)obj2);
            return obj1;
        }


        #region Helpers
        /// <summary>
        /// Gets the description attribute of the enumeration type.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="value">The specified enumeration value.</param>
        /// <param name="splitByUpperCase">Whether the description should contain spaces between each capital letter.</param>
        /// <returns>he description attribute of the enumeration value.</returns>
        public static string GetEnumDescription<T>(this T value, bool splitByUpperCase = false) where T : struct, IConvertible, IFormattable //The compiler doesnt allow [where T: System.Enum]
        {
            if ((object)value == null) return "";

            var type = value.GetType();

            var result = new List<string>();
            var values = value.ToFlagsList<T>();   //Flags

            foreach (T enumValue in values)
            {
                var v = enumValue.ToString();
                var fi = type.GetField(v);
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                result.Add(attributes.Length > 0 ? attributes[0].Description : splitByUpperCase ? v.SplitByUppercase() : v.ToSentenceCase(true));
            }

            return String.Join(", ", result);
        }

        /// <summary>
        /// Retrieves an array of description attributes for the specified enumeration type. 
        /// </summary>
        /// <returns>An array of description attributes for the specified enumeration type.</returns>
        public static string[] GetDescriptions<T>() where T : struct, IConvertible, IFormattable //The compiler doesnt allow [where T: System.Enum]
        {
            var enumValArray = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

            var result = new string[enumValArray.Length];
            for (var i = 0; i < enumValArray.Length; i++)
            {
                result[i] = GetEnumDescription(enumValArray[i]);
            }
            return result;
        }


        /// <summary>
        /// Retrieves an array of description attributes for the specified enumeration type. 
        /// </summary>
        /// <param name="enumType">The specified enumeration type.</param>
        /// <returns>An array of description attributes for the specified enumeration type.</returns>
        public static string[] GetDescriptions(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<Enum>()
                    .Select(v => v.GetDescription()).ToArray();
        }

        #endregion
    }
}
