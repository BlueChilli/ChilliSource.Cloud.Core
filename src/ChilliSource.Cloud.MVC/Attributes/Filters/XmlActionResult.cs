using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents an attribute that sets content type to "text/xml" for the response.
    /// </summary>
    public class XmlActionResult : ActionFilterAttribute
    {
        /// <summary>
        /// Sets content type to "text/xml" for the response.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is ViewResult)
            {
                var result = filterContext.Result as ViewResult;
                if (!String.IsNullOrEmpty(result.ViewName))
                {
                    filterContext.Result = new ContentResult
                    {
                        Content = result.ViewName.ToString(),
                        ContentType = "text/xml",
                        ContentEncoding = System.Text.Encoding.UTF8
                    };
                }
            }
        }

    }
}