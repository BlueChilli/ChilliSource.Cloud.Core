
using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for the CSS style element.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="filename">The CSS style file name.</param>
        /// <returns>An HTML string for the CSS style element.</returns>
        public static MvcHtmlString Style(this HtmlHelper html, string filename)
        {
            string format = @"<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />";

            return new MvcHtmlString(String.Format(format, ResolveFilenameToUrl(html, DirectoryType.Styles, filename)));
        }
    }
}