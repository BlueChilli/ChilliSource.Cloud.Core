using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// See RouteValueDictionaryExtensions in BlueChilli.Lib for additional functions.
    /// </summary>
    public class RouteValueDictionaryHelper
    {
        /// <summary>
        /// Converts html attribute object to System.Web.Routing.RouteValueDictionary.
        /// </summary>
        /// <param name="htmlAttributes">The specified html attribute object.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        public static RouteValueDictionary CreateFromHtmlAttributes(object htmlAttributes)
        {
            var result = new RouteValueDictionary();

            if (htmlAttributes == null)
                return result;

            if (htmlAttributes is RouteValueDictionary)
                result = htmlAttributes as RouteValueDictionary;
            else if (htmlAttributes is IDictionary<string, object>)
                result = new RouteValueDictionary(htmlAttributes as IDictionary<string, object>);
            else 
                result = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (result.ContainsKey("disabled") && result["disabled"] is Boolean && ((bool)result["disabled"]) == false)
            {
                result.Remove("disabled");
            }

            return result;
        }
    }
}