
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for System.Web.Routing.RequestContext.
    /// </summary>
    public static class RequestContextExtensions
    {
        /// <summary>
        /// Transforms the specified URL with URL parameters from the route in specified request context.
        /// </summary>
        /// <param name="request">The specified request context.</param>
        /// <param name="alternativeUrl">The URL to transform.</param>
        /// <returns>A URL transformed by URL parameters from the route in specified request context.</returns>
        public static string TransformUrl(this RequestContext request, string alternativeUrl = "")
        {
            if (alternativeUrl.Equals(String.Empty))
            {
                alternativeUrl = alternativeUrl.DefaultTo(request.HttpContext.Request.Url.PathAndQuery);
            }
            else
            {
                alternativeUrl = alternativeUrl.TransformWith(request.RouteData.Values.ToDictionary());
            }
            return alternativeUrl;
        }
    }
}