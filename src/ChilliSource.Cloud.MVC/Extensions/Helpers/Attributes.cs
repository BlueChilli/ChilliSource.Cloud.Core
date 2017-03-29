using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns string for HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML-encoded string for HTML attributes.</returns>
        public static MvcHtmlString Attributes<TModel>(this HtmlHelper<TModel> html, object htmlAttributes)
        {
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
            var sb = new StringBuilder();
            foreach (var key in attributes.Keys)
            {
                sb.AppendFormat(@" {0}=""{1}""", key, attributes[key]);
            }
            return MvcHtmlString.Create(sb.ToString().Trim());
        }
    }
}