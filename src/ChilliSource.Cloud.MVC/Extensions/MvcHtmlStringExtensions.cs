using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for System.Web.Mvc.MvcHtmlString.
    /// </summary>
    public static class MvcHtmlStringExtensions
    {
        /// <summary>
        /// Creates a new HTML-encoded string by formatting the specified HTML-encoded string.
        /// </summary>
        /// <param name="value">The specified HTML-encoded string.</param>
        /// <param name="format">A composite format string.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static MvcHtmlString Format(this MvcHtmlString value, string format)
        {
            return MvcHtmlString.Create(String.Format(format, value.ToHtmlString()));
        }

        /// <summary>
        /// Creates a new HTML-encoded string by formatting the specified HTML-encoded string.
        /// </summary>
        /// <param name="value">The specified HTML-encoded string.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static MvcHtmlString Format(this MvcHtmlString value, string format, params object[] args)
        {
            return MvcHtmlString.Create(String.Format(format, args));
        }

        /// <summary>
        /// Creates a new HTML-encoded string by formatting the specified HTML-encoded string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="values">An object array that contains zero or more objects to format.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static MvcHtmlString Format(string format, params object[] values)
        {
            return MvcHtmlString.Create(String.Format(format, values));
        }
    }
}