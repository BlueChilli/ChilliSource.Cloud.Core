
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Converts collections into SelectList objects
    /// </summary>
    public static class SelectListExtensions
    {
        /// <summary>
        /// Converts a Enum collection into a SelectList object.
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="list">List of enum values</param>
        /// <param name="value">Select value</param>
        /// <returns>Converted SelectList object</returns>
        public static SelectList ToSelectList<TEnum>(this List<TEnum> list, TEnum? value = null) where TEnum : struct
        {
            var items = new List<SelectListItem>();
            foreach (var item in list)
            {
                var enumItem = item as Enum;
                items.Add(new SelectListItem { Value = enumItem.ToString(), Text = enumItem.GetDescription() });
            }
            return new SelectList(items, "Value", "Text", value.HasValue ? value.Value.ToString() : null);
        }

        /// <summary>
        /// Converts a String collection into a SelectList object
        /// </summary>
        /// <param name="list">List of strings</param>
        /// <returns>Converted SelectList object</returns>
        public static SelectList ToSelectList(this List<string> list)
        {
            return new SelectList(list.Select(x => new KeyValuePair<string, string>(x, x)), "Key", "Value");
        }

        /// <summary>
        /// Converts a collection into a SelectList object
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <typeparam name="TValue">Value type. Must be convertible to String.</typeparam>
        /// <typeparam name="TText">Text type. Must be convertible to String.</typeparam>
        /// <param name="collection">List of elements</param>
        /// <param name="valueFunc">Anonymous function to get the item value</param>
        /// <param name="textFunc">Anonymous function to get the display value</param>
        /// <param name="value">Current selected value</param>
        /// <returns>Converted SelectList object</returns>
        public static SelectList ToSelectList<T, TValue, TText>(this IEnumerable<T> collection, Func<T, TValue> valueFunc, Func<T, TText> textFunc, TValue value = default(TValue))
        {
            var kvpList = new List<KeyValuePair<string, string>>();
            foreach (var item in collection)
            {
                kvpList.Add(new KeyValuePair<string, string>(valueFunc(item).ToString(), textFunc(item).ToString()));
            }
            return new SelectList(kvpList, "Key", "Value", value);
        }

        /// <summary>
        /// Converts a collection into a SelectList object
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <typeparam name="TValue">Value type. Must be convertible to String.</typeparam>
        /// <typeparam name="TText">Text type. Must be convertible to String.</typeparam>
        /// <param name="collection">List of elements</param>
        /// <param name="valueFunc">Anonymous function to get the item value</param>
        /// <param name="textFunc">Anonymous function to get the display value</param>
        /// <param name="groupFunc">Anonymous function to get the group value (optgroup in html)</param>
        /// <param name="value">Current selected value</param>
        /// <returns>Converted SelectList object</returns>
        public static SelectList ToSelectList<T, TValue, TText, TGroup>(this IEnumerable<T> collection,
            Func<T, TValue> valueFunc,
            Func<T, TText> textFunc,
            Func<T, TGroup> groupFunc,
            TValue value = default(TValue))
        {
            var list = collection.Select(c => new { Key = valueFunc(c).ToString(), Value = textFunc(c).ToString(), Group = groupFunc(c).ToString() });
            return new SelectList(list, "Key", "Value", "Group", value);
        }
        /// <summary>
        /// Converts 2 collections containing values and display texts into a SelectList object
        /// </summary>
        /// <param name="valueList">List of values</param>
        /// <param name="textList">List of display texts</param>
        /// <returns>Converted SelectList object</returns>
        public static SelectList ToSelectList(this IEnumerable<string> valueList, IEnumerable<string> textList)
        {
            var kvpList = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < valueList.Count(); i++)
            {
                kvpList.Add(new KeyValuePair<string, string>(valueList.ElementAt(i), textList.ElementAt(i)));
            }
            return new SelectList(kvpList, "Key", "Value");
        }

        /// <summary>
        /// Converts a list of SelectListItem into a SelectList object
        /// </summary>
        /// <param name="list">List of elements</param>
        /// <returns>Converted SelectList object</returns>
        public static SelectList ToSelectList(this IEnumerable<SelectListItem> list)
        {
            return new SelectList(list, "Value", "Text");
        }
    }
}
