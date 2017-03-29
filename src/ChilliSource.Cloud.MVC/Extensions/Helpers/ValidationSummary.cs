using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for validation summary message.
        /// </summary>
        /// <param name="html">An object that contains the HTML attributes.</param>
        /// <param name="validationMessage">Validation message.</param>
        /// <returns>An HTML string for validation summary message.</returns>
        public static MvcHtmlString ValidationSummaryHtml(this HtmlHelper html, string validationMessage)
        {
            var code = html.ValidationSummary(validationMessage);
            return MvcHtmlString.Create(HttpUtility.HtmlDecode(code.ToHtmlString()));
        }
    }
}