using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChilliSource.Cloud.Web
{
    /// <summary>
    /// Extension methods for System.Uri.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Gets the absolute URI with "http" protocol.
        /// </summary>
        /// <param name="uri">A System.Uri.</param>
        /// <returns>A System.String containing the entire URI.</returns>
        public static string ToLink(this Uri uri)
        {
            return uri.AbsoluteUri.Replace("https:", "").Replace("http:", "");
        }

        /// <summary>
        /// Gets the host component of the specified System.Uri, without "www.".
        /// </summary>
        /// <param name="uri">A System.Uri.</param>
        /// <returns>The host component of the URI.</returns>
        public static string FriendlyName(this Uri uri)
        {
            if (uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                return uri.Host.Substring(4);
            }
            return uri.Host;
        }

        /// <summary>
        /// Gets the root of the specified System.Uri.
        /// </summary>
        /// <param name="uri">A System.Uri.</param>
        /// <returns>The root component of the URI.</returns>
        public static string Root(this Uri uri)
        {
            return uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.Length - uri.AbsolutePath.Length);
        }

        /// <summary>
        /// Converts System.Uri to URI string with query string parameters and appends and empty query string parameter.
        /// </summary>
        /// <param name="uri">A System.Uri.</param>
        /// <param name="appendAsEmptyQuery">The key for empty query string parameter.</param>
        /// <returns>A URI string with query string parameters and appends and empty query string parameter.</returns>
        public static string ToString(this Uri uri, string appendAsEmptyQuery)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            queryString.Remove(appendAsEmptyQuery); //if (queryString.AllKeys.Contains(appendAsEmptyQuery)) 
            return String.Format("{0}?{1}{2}{3}=", uri.AbsolutePath, queryString.ToString(), queryString.Count > 0 ? "&" : "", appendAsEmptyQuery);
        }

        /// <summary>
        /// Adds object property name and value pairs as query string parameters to specified Syste.Uri.
        /// </summary>
        /// <param name="uri">A System.Uri.</param>
        /// <param name="parameters">The specified object.</param>
        /// <returns>A System.Uri with query string parameters from the specified object properties.</returns>
        public static Uri AddQuery(this Uri uri, object parameters)
        {
            var queryString = HttpUtility.ParseQueryString(uri.Query);

            Type t = parameters.GetType();
            foreach (PropertyInfo property in t.GetProperties())
            {
                queryString[property.Name] = property.GetValue(parameters, null) == null ? "" : property.GetValue(parameters, null).ToString();
            }

            return Parse(String.Format("{0}?{1}", uri.AbsolutePath, queryString.ToString()));
        }

        /// <summary>
        /// Converts a string of relative URL to System.Uri.
        /// </summary>
        /// <param name="url">The relative URL string.</param>
        /// <param name="current">Current HTTP context.</param>
        /// <returns>A System.Uri.</returns>
        public static Uri Parse(string url, HttpContext current = null)
        {
            if (current == null) current = HttpContext.Current;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) return new Uri(url);
            string authority = current == null ? "http://" + Dns.GetHostName() : current.Request.Url.GetLeftPart(UriPartial.Authority);
            return new Uri(authority + VirtualPathUtility.ToAbsolute(url));
        }

        /// <summary>
        /// Converts a string of relative URL to System.Uri and adds object property name and value pairs as query string parameters.
        /// </summary>
        /// <param name="url">The relative URL string.</param>
        /// <param name="parameters">The specified object.</param>
        /// <returns>A System.Uri with query string parameters from the specified object properties.</returns>
        public static Uri Parse(string url, object parameters)
        {
            var uri = Parse(url);
            return uri.AddQuery(parameters);
        }
    }
}
