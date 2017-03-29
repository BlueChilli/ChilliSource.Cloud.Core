using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string using the specified text when condition is true.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="condition">True to use the specified text, otherwise not.</param>
        /// <param name="result">The specified text.</param>
        /// <returns>An HTML string using the specified text</returns>
        public static MvcHtmlString When(this HtmlHelper htmlHelper, bool condition, string result)
        {
            return condition ? MvcHtmlString.Create(result) : MvcHtmlString.Empty;
        }
    }
}