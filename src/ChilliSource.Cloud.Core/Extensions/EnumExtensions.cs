using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using ChilliSource.Cloud.Core;

namespace ChilliSource.Cloud.Core
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


        /// <summary>
        /// Retrieves an array list of the values of the constants in a specified enumeration.
        /// </summary>
        /// <param name="enumType">The specified enumeration value.</param>
        /// /// <param name="excludeObsolete">Specifies whether to exclude obsolete enum values.</param>
        /// <returns>An array list that contains the values of the constants in enumType.</returns>
        public static object[] GetValues(Type enumType, bool excludeObsolete = true)
        {
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            var values = Enum.GetValues(enumType).Cast<Enum>();
            if (excludeObsolete)
                values = values.Where(value =>
                            {
                                var fieldInfo = enumType.GetField(Enum.GetName(enumType, value));
                                return !fieldInfo.GetCustomAttributes<ObsoleteAttribute>().Any();
                            });

            return values.ToArray();
        }
    }
}
