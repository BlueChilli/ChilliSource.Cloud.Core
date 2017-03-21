using ChilliSource.Cloud.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web
{
    /// <summary>
    /// Extension methods for System.Web.Routing.RouteValueDictionary.
    /// </summary>
    public static class RouteValueDictionaryExtensions
    {
        /// <summary>
        /// Adds the key and value to specified route value dictionary if key does not exist, otherwise appends value.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="key">The key to retrieve value.</param>
        /// <param name="value">The value string.</param>
        public static void Merge(this RouteValueDictionary dictionary, string key, object value)
        {
            if (dictionary.ContainsKey(key))
            {
                if (value is string)
                {
                    dictionary[key] += " " + value.ToString();
                }
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Merges the second route value dictionary into the first route value dictionary.
        /// </summary>
        /// <param name="dictionary">The first System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="dictionary2">The second System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="overwrite">True to overwrite value in first dictionary from the second dictionary having same key, otherwise not.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        public static RouteValueDictionary Merge(this RouteValueDictionary dictionary, RouteValueDictionary dictionary2, bool overwrite = false)
        {
            foreach (var key in dictionary2.Keys)
            {
                if (!dictionary.ContainsKey(key)) dictionary.Add(key, dictionary2[key]);
                else if (overwrite) dictionary[key] = dictionary2[key];
            }
            return dictionary;
        }

        /// <summary>
        /// Converts System.Web.Routing.RouteValueDictionary to System.Collections.Generic.Dictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>A System.Collections.Generic.Dictionary&lt;string, object&gt;.</returns>
        public static Dictionary<string, object> ToDictionary(this RouteValueDictionary dictionary)
        {
            var result = new Dictionary<string, object>();
            foreach (var item in dictionary)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        /// <summary>
        /// Returns only the parameters of a route.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="query">Optionally mix in a query string into the result</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        /// <example>HttpContext.Current.Request.RequestContext.RouteData.Values.Parameters(HttpContext.Current.Request.Url.Query)</example>
        public static RouteValueDictionary Parameters(this RouteValueDictionary dictionary, string query = null)
        {
            dictionary = new RouteValueDictionary(dictionary.ToDictionary());
            foreach (string key in RouteHelper.Keys)
            {
                if (dictionary.ContainsKey(key)) dictionary.Remove(key);
            }
            if (!String.IsNullOrEmpty(query))
            {
                dictionary.AddQuery(query);
            }
            return dictionary;
        }

        /// <summary>
        /// Returns only the parameters of a route with additional route values specified.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="routeValues">Route values to be merged.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        public static RouteValueDictionary Parameters(this RouteValueDictionary dictionary, object routeValues)
        {
            dictionary = dictionary.Parameters();
            dictionary.Merge(new RouteValueDictionary(routeValues), overwrite: true);
            return dictionary;
        }

        /// <summary>
        /// Adds route values from query string to specified route value dictionary.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="query">Query string to add.</param>
        public static void AddQuery(this RouteValueDictionary dictionary, string query)
        {
            if (String.IsNullOrEmpty(query)) return;
            var queryString = HttpUtility.ParseQueryString(query);
            foreach (string key in queryString.AllKeys)
            {
                if (!String.IsNullOrEmpty(key)) dictionary.Add(key, queryString[key]);
            }
        }

        /// <summary>
        ///  Creates a new instance of the System.Web.Routing.RouteValueDictionary class and adds values that are based on properties from the specified object.
        /// </summary>
        /// <param name="routeValues">An object that contains properties that will be added as elements to the new collection.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        public static RouteValueDictionary Create(object routeValues)
        {
            if (routeValues == null) return new RouteValueDictionary();
            if (routeValues is RouteValueDictionary) return (RouteValueDictionary)routeValues;
            return new RouteValueDictionary(routeValues);
        }

        /// <summary>
        /// Converts System.Web.Routing.RouteValueDictionary to attribute string.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>Attribute string (E.g. key1="value1" key2="value2")</returns>
        public static string ToAttributeString(this RouteValueDictionary dictionary)
        {
            return String.Join(" ", dictionary.Keys.Select(
                key => String.Format("{0}=\"{1}\"", key,
                dictionary[key])));
        }

        /// <summary>
        /// Converts System.Web.Routing.RouteValueDictionary to JSON string.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <returns>JSON string representing the route value dictionary.</returns>
        public static string ToJsonString(this RouteValueDictionary dictionary)
        {
            return "{" + String.Join(",", dictionary.Keys.Select(
                key => String.Format("\"{0}\":\"{1}\"", key, dictionary[key])))
            + "}";
        }

        /// <summary>
        /// Adds the key and value to specified route value dictionary only when key dose not exist in the dictionary.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="key">The key to retrieve value.</param>
        /// <param name="value">The value string.</param>
        public static void AddOrSkipIfExists(this RouteValueDictionary dictionary, string key, object value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
        }

        /// <summary>
        /// Copies a route with new parameters.
        /// </summary>
        /// <param name="dictionary">The System.Web.Routing.RouteValueDictionary.</param>
        /// <param name="routeValues">Route values to be copy.</param>
        /// <param name="overWrite">True to overwrite value in the dictionary from the route value object having same key, otherwise not.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        /// <example>route.CopyWith(new { Criteria = SearchCriteria.Booked })</example>
        public static RouteValueDictionary CopyWith(this RouteValueDictionary dictionary, object routeValues, bool overWrite = false)
        {
            var dictionary2 = RouteValueDictionaryExtensions.Create(routeValues);
            var dictionaryCopy = new RouteValueDictionary(dictionary.ToDictionary());
            return dictionaryCopy.Merge(dictionary2, overWrite);
        }
    }
}
